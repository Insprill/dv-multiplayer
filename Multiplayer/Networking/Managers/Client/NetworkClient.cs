using System.Net;
using DV;
using DV.UI;
using LiteNetLib;
using LiteNetLib.Utils;
using Multiplayer.Networking.Packets.Clientbound;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Serverbound;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Networking.Listeners;

public class NetworkClient : NetworkManager
{
    private byte localId;
    private NetPeer serverPeer;
    private readonly ClientPlayerManager playerManager;

    public NetworkClient(Settings settings) : base(settings)
    {
        playerManager = new ClientPlayerManager();
    }

    public void Start(string address, int port, string password)
    {
        netManager.Start();
        netManager.Connect(address, port, password);
    }

    protected override void Subscribe()
    {
        netPacketProcessor.SubscribeReusable<ClientAcceptedPacket, NetPeer>(OnClientAcceptedPacket);
        netPacketProcessor.SubscribeReusable<ClientJoinedPacket, NetPeer>(OnClientJoinedPacket);
        netPacketProcessor.SubscribeReusable<ClientLeftPacket, NetPeer>(OnClientLeftPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerPositionPacket, NetPeer>(OnClientboundPlayerPositionPacket);
    }

    #region Common

    public override void OnPeerConnected(NetPeer peer)
    {
        serverPeer = peer;
        ServerboundClientInfoPacket serverboundClientInfoPacket = new() {
            Username = Multiplayer.Settings.Username,
            BuildMajorVersion = (ushort)BuildInfo.BUILD_VERSION_MAJOR,
            Mods = ModInfo.FromModEntries(UnityModManager.modEntries)
        };
        NetDataWriter writer = new();
        netPacketProcessor.Write(writer, serverboundClientInfoPacket);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        // todo: popup, read additional info from disconnectInfo
        MainMenu.GoBackToMainMenu();
    }

    public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        // todo
    }

    public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        // todo
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        // todo
    }

    #endregion

    #region Listeners

    private void OnClientAcceptedPacket(ClientAcceptedPacket packet, NetPeer peer)
    {
        localId = packet.Id;
        AStartGameData.FallbackNewCareer();
        SceneSwitcher.SwitchToScene(DVScenes.Game);
    }

    private void OnClientJoinedPacket(ClientJoinedPacket packet, NetPeer peer)
    {
        if (packet.Id == localId)
            return;
        playerManager.AddPlayer(packet.Id, packet.Username);
    }

    private void OnClientLeftPacket(ClientLeftPacket packet, NetPeer peer)
    {
        if (packet.Id == localId)
            return;
        playerManager.RemovePlayer(packet.Id);
    }

    private void OnClientboundPlayerPositionPacket(ClientboundPlayerPositionPacket packet, NetPeer peer)
    {
        if (packet.Id == localId)
            return;
        playerManager.UpdatePosition(packet.Id, packet.Position, packet.RotationY);
    }

    #endregion

    #region Senders

    public void SendPlayerPosition(Vector3 newPosition, float newRotationY)
    {
        SendPacket(serverPeer, new ServerboundPlayerPositionPacket {
            Position = newPosition,
            RotationY = newRotationY
        }, DeliveryMethod.Sequenced);
    }

    #endregion
}
