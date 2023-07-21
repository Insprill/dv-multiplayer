using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DV;
using DV.WeatherSystem;
using LiteNetLib;
using LiteNetLib.Utils;
using Multiplayer.Components.Networking;
using Multiplayer.Networking.Packets.Clientbound;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Serverbound;
using UnityModManagerNet;

namespace Multiplayer.Networking.Listeners;

public class NetworkServer : NetworkManager
{
    private readonly Dictionary<byte, ServerPlayer> serverPlayers = new();
    private readonly Dictionary<byte, NetPeer> netPeers = new();

    private NetPeer selfPeer => NetworkLifecycle.Instance.Client?.selfPeer;
    private readonly ModInfo[] serverMods;

    public NetworkServer(Settings settings) : base(settings)
    {
        serverMods = ModInfo.FromModEntries(UnityModManager.modEntries);
    }

    public void Start(int port)
    {
        netManager.Start(port);
    }

    protected override void Subscribe()
    {
        netPacketProcessor.SubscribeReusable<ServerboundClientLoginPacket, ConnectionRequest>(OnServerboundClientLoginPacket);
        netPacketProcessor.SubscribeReusable<ServerboundClientReadyPacket, NetPeer>(OnServerboundClientReadyPacket);
        netPacketProcessor.SubscribeReusable<ServerboundPlayerPositionPacket, NetPeer>(OnServerboundPlayerPositionPacket);
        netPacketProcessor.SubscribeReusable<ServerboundTimeAdvancePacket, NetPeer>(OnServerboundTimeAdvancePacket);
        netPacketProcessor.SubscribeReusable<CommonChangeJunctionPacket>(OnCommonChangeJunctionPacket);
        netPacketProcessor.SubscribeReusable<CommonRotateTurntablePacket>(OnCommonRotateTurntablePacket);
    }

    public bool TryGetServerPlayer(NetPeer peer, out ServerPlayer player)
    {
        return serverPlayers.TryGetValue((byte)peer.Id, out player);
    }

    public bool TryGetNetPeer(byte id, out NetPeer peer)
    {
        return netPeers.TryGetValue(id, out peer);
    }

    #region Overrides

    public override void OnPeerConnected(NetPeer peer)
    { }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        byte id = (byte)peer.Id;
        serverPlayers.Remove(id);
        netPeers.Remove(id);
        netManager.SendToAll(WritePacket(new ClientboundPlayerDisconnectPacket {
            Id = id
        }), DeliveryMethod.ReliableUnordered);
    }

    public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        // todo
    }

    public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        if (TryGetServerPlayer(peer, out ServerPlayer player))
            player.Ping = latency;

        ClientboundPingUpdatePacket clientboundPingUpdatePacket = new() {
            Id = (byte)peer.Id,
            Ping = latency
        };

        SendPacketToAll(clientboundPingUpdatePacket, DeliveryMethod.ReliableOrdered, peer);

        SendPacket(peer, new ClientboundTimeSyncPacket {
            ServerTime = DateTime.UtcNow.Millisecond
        }, DeliveryMethod.ReliableOrdered);
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        netPacketProcessor.ReadAllPackets(request.Data, request);
    }

    #endregion

    #region Packet Senders

    private void SendPacketToAll<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
    {
        NetDataWriter writer = WritePacket(packet);
        foreach (KeyValuePair<byte, NetPeer> kvp in netPeers)
            kvp.Value.Send(writer, deliveryMethod);
    }

    private void SendPacketToAll<T>(T packet, DeliveryMethod deliveryMethod, NetPeer excludePeer) where T : class, new()
    {
        NetDataWriter writer = WritePacket(packet);
        foreach (KeyValuePair<byte, NetPeer> kvp in netPeers)
        {
            if (kvp.Key == excludePeer.Id)
                continue;
            kvp.Value.Send(writer, deliveryMethod);
        }
    }

    #endregion

    private void OnServerboundClientLoginPacket(ServerboundClientLoginPacket packet, ConnectionRequest request)
    {
        Log($"Processing login packet{(Multiplayer.Settings.LogIps ? $" from ({request.RemoteEndPoint.Address})" : "")}");

        if (Multiplayer.Settings.Password != packet.Password)
        {
            LogWarning("Denied login due to invalid password!");
            ClientboundServerDenyPacket denyPacket = new() {
                Reason = "Invalid password!"
            };
            request.Reject(WritePacket(denyPacket));
            return;
        }

        if (packet.BuildMajorVersion != BuildInfo.BUILD_VERSION_MAJOR)
        {
            LogWarning($"Denied login to incorrect game version! Got: {packet.BuildMajorVersion}, expected: {BuildInfo.BUILD_VERSION_MAJOR}");
            ClientboundServerDenyPacket denyPacket = new() {
                Reason = $"Game version mismatch! Server build: {BuildInfo.BUILD_VERSION_MAJOR}, your build: {packet.BuildMajorVersion}."
            };
            request.Reject(WritePacket(denyPacket));
            return;
        }

        if (netManager.ConnectedPeersCount >= Multiplayer.Settings.MaxPlayers)
        {
            LogWarning("Denied login due to server being full!");
            ClientboundServerDenyPacket denyPacket = new() {
                Reason = "Server is full!"
            };
            request.Reject(WritePacket(denyPacket));
            return;
        }

        ModInfo[] clientMods = packet.Mods;
        if (!serverMods.SequenceEqual(clientMods))
        {
            ModInfo[] missing = serverMods.Except(clientMods).ToArray();
            ModInfo[] extra = clientMods.Except(serverMods).ToArray();
            LogWarning($"Denied login due to mod mismatch! {missing.Length} missing, {extra.Length} extra");
            ClientboundServerDenyPacket denyPacket = new() {
                Reason = "Mod mismatch!",
                Missing = missing,
                Extra = extra
            };
            request.Reject(WritePacket(denyPacket));
            return;
        }

        NetPeer peer = request.Accept();

        ServerPlayer serverPlayer = new() {
            Id = (byte)peer.Id,
            Username = packet.Username
        };

        serverPlayers.Add(serverPlayer.Id, serverPlayer);
    }

    private void OnServerboundClientReadyPacket(ServerboundClientReadyPacket packet, NetPeer peer)
    {
        byte peerId = (byte)peer.Id;

        // Allow the player to receive packets
        netPeers.Add(peerId, peer);

        // Send the new player to all other players
        ServerPlayer serverPlayer = serverPlayers[peerId];
        ClientboundPlayerJoinedPacket clientboundPlayerJoinedPacket = new() {
            Id = peerId,
            Username = serverPlayer.Username
        };
        SendPacketToAll(clientboundPlayerJoinedPacket, DeliveryMethod.ReliableOrdered, peer);

        Log($"Client {peer.Id} is ready. Sending world state");

        // No need to sync the world state if the player is the host
        if (NetworkLifecycle.Instance.IsHost(peer))
        {
            SendPacket(peer, new ClientboundRemoveLoadingScreenPacket(), DeliveryMethod.ReliableOrdered);
            return;
        }

        SendPacket(peer, new ClientboundBeginWorldSyncPacket(), DeliveryMethod.ReliableOrdered);

        // Send existing players
        foreach (ServerPlayer player in serverPlayers.Values)
        {
            if (player.Id == peer.Id)
                continue;
            SendPacket(peer, new ClientboundPlayerJoinedPacket {
                Id = player.Id,
                Username = player.Username
            }, DeliveryMethod.ReliableOrdered);
        }

        // Send weather state
        SendPacket(peer, WeatherDriver.Instance.GetSaveData().ToObject<ClientboundWeatherPacket>(), DeliveryMethod.ReliableOrdered);

        // Send junctions
        SendPacket(peer, new ClientboundJunctionStatePacket {
            selectedBranches = WorldData.Instance.OrderedJunctions.Select(j => (byte)j.selectedBranch).ToArray()
        }, DeliveryMethod.ReliableOrdered);

        // Send turntables
        SendPacket(peer, new ClientboundTurntableStatePacket {
            rotations = TurntableController.allControllers.Select(c => c.turntable.currentYRotation).ToArray()
        }, DeliveryMethod.ReliableOrdered);

        // All data has been sent, allow the client to load into the world.
        SendPacket(peer, new ClientboundRemoveLoadingScreenPacket(), DeliveryMethod.ReliableOrdered);
    }

    private void OnServerboundPlayerPositionPacket(ServerboundPlayerPositionPacket packet, NetPeer peer)
    {
        ClientboundPlayerPositionPacket clientboundPacket = new() {
            Id = (byte)peer.Id,
            Position = packet.Position,
            RotationY = packet.RotationY,
            IsJumping = packet.IsJumping
        };

        SendPacketToAll(clientboundPacket, DeliveryMethod.Sequenced, peer);
    }

    private void OnServerboundTimeAdvancePacket(ServerboundTimeAdvancePacket packet, NetPeer peer)
    {
        SendPacketToAll(new ClientboundTimeAdvancePacket {
            amountOfTimeToSkipInSeconds = packet.amountOfTimeToSkipInSeconds
        }, DeliveryMethod.ReliableOrdered, peer);
    }

    private void OnCommonChangeJunctionPacket(CommonChangeJunctionPacket packet)
    {
        SendPacketToAll(packet, DeliveryMethod.ReliableOrdered);
    }

    private void OnCommonRotateTurntablePacket(CommonRotateTurntablePacket packet)
    {
        SendPacketToAll(packet, DeliveryMethod.ReliableOrdered);
    }

    #region Logging

    private static void Log(object msg)
    {
        Multiplayer.Log($"[Server] {msg}");
    }

    private static void LogWarning(object msg)
    {
        Multiplayer.LogWarning($"[Server] {msg}");
    }

    #endregion
}
