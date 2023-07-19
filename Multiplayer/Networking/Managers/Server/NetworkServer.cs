using System.Collections.Generic;
using System.Linq;
using System.Net;
using DV;
using LiteNetLib;
using Multiplayer.Networking.Packets.Clientbound;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Serverbound;
using UnityModManagerNet;

namespace Multiplayer.Networking.Listeners;

public class NetworkServer : NetworkManager
{
    private readonly Dictionary<byte, ServerPlayer> serverPlayers = new();
    private readonly Dictionary<byte, NetPeer> netPeers = new();
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
    }

    public bool TryGetServerPlayer(NetPeer peer, out ServerPlayer player)
    {
        return serverPlayers.TryGetValue((byte)peer.Id, out player);
    }

    public bool TryGetNetPeer(byte id, out NetPeer peer)
    {
        return netPeers.TryGetValue(id, out peer);
    }

    #region Common

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

        netManager.SendToAll(WritePacket(clientboundPingUpdatePacket), DeliveryMethod.ReliableOrdered, peer);
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        netPacketProcessor.ReadAllPackets(request.Data, request);
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
                Reason = "Server is full!"
            };
            request.Reject(WritePacket(denyPacket));
        }

        if (netManager.ConnectedPeersCount >= Multiplayer.Settings.MaxPlayers)
        {
            LogWarning("Denied login due to server being full!");
            ClientboundServerDenyPacket denyPacket = new() {
                Reason = "Server is full!"
            };
            request.Reject(WritePacket(denyPacket));
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

        Log("Login accepted! Broadcasting to all clients");

        NetPeer peer = request.Accept();
        byte peerId = (byte)peer.Id;

        ServerPlayer serverPlayer = new() {
            Id = peerId,
            Username = packet.Username
        };

        serverPlayers.Add(peerId, serverPlayer);
        netPeers.Add(peerId, peer);

        ClientboundPlayerJoinedPacket clientboundPlayerJoinedPacket = new() {
            Id = peerId,
            Username = packet.Username
        };

        netManager.SendToAll(WritePacket(clientboundPlayerJoinedPacket), DeliveryMethod.ReliableUnordered, peer);
    }

    private void OnServerboundClientReadyPacket(ServerboundClientReadyPacket packet, NetPeer peer)
    {
        Log($"Client {peer.Id} is ready. Sending world state");
        foreach (ServerPlayer player in serverPlayers.Values)
        {
            if (player.Id == peer.Id)
                continue;
            SendPacket(peer, new ClientboundPlayerJoinedPacket {
                Id = player.Id,
                Username = player.Username
            }, DeliveryMethod.ReliableUnordered);
        }
    }

    private void OnServerboundPlayerPositionPacket(ServerboundPlayerPositionPacket packet, NetPeer peer)
    {
        ClientboundPlayerPositionPacket clientboundPacket = new() {
            Id = (byte)peer.Id,
            Position = packet.Position,
            RotationY = packet.RotationY,
            IsJumping = packet.IsJumping
        };

        netManager.SendToAll(WritePacket(clientboundPacket), DeliveryMethod.Sequenced, peer);
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
