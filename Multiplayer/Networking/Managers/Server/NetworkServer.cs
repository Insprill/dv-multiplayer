using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    public NetworkServer(Settings settings) : base(settings)
    { }

    public void Start(int port)
    {
        netManager.Start(port);
    }

    protected override void Subscribe()
    {
        netPacketProcessor.SubscribeReusable<ServerboundClientInfoPacket, NetPeer>(OnClientInfoPacket);
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
        serverPlayers.Remove((byte)peer.Id);
        netPeers.Remove((byte)peer.Id);
    }

    public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        // todo
    }

    public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        if (TryGetServerPlayer(peer, out ServerPlayer player))
            player.Ping = latency;

        ClientPingUpdatePacket clientPingUpdatePacket = new() {
            Id = (byte)peer.Id,
            Ping = latency
        };

        netManager.SendToAll(WritePacket(clientPingUpdatePacket), DeliveryMethod.ReliableOrdered);
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        if (netManager.ConnectedPeersCount < Multiplayer.Settings.MaxPlayers)
            request.AcceptIfKey(Multiplayer.Settings.Password);
        else
            request.Reject(); //todo: send packet with reason
    }

    #endregion

    private void OnClientInfoPacket(ServerboundClientInfoPacket packet, NetPeer peer)
    {
        byte peerId = (byte)peer.Id;

        if (netPeers.ContainsKey(peerId))
        {
            Multiplayer.LogError("Client sent ClientInfoPacket twice!");
            peer.Disconnect();
            return;
        }

        ModInfo[] serverMods = ModInfo.FromModEntries(UnityModManager.modEntries);
        ModInfo[] clientMods = packet.Mods;

        if (!serverMods.SequenceEqual(clientMods))
        {
            ModMismatchPacket modMismatchPacket = new() {
                Missing = serverMods.Except(clientMods).ToArray(),
                Extra = clientMods.Except(serverMods).ToArray()
            };

            peer.Disconnect(WritePacket(modMismatchPacket));
            return;
        }

        ServerPlayer serverPlayer = new() {
            Id = peerId,
            Username = packet.Username
        };

        serverPlayers.Add(peerId, serverPlayer);
        netPeers.Add(peerId, peer);

        ClientJoinedPacket clientJoinedPacket = new() {
            Id = peerId,
            Username = packet.Username
        };

        netManager.SendToAll(WritePacket(clientJoinedPacket), DeliveryMethod.ReliableOrdered);

        SendPacket(peer, new ClientAcceptedPacket(), DeliveryMethod.ReliableOrdered);
    }

    private void OnServerboundPlayerPositionPacket(ServerboundPlayerPositionPacket packet, NetPeer peer)
    {
        ClientboundPlayerPositionPacket clientboundPacket = new() {
            Id = (byte)peer.Id,
            Position = packet.Position,
            RotationY = packet.RotationY
        };

        netManager.SendToAll(WritePacket(clientboundPacket), DeliveryMethod.Sequenced);
    }
}
