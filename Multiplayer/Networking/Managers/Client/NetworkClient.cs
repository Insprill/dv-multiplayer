using System.Collections;
using System.Net;
using DV;
using DV.UI;
using DV.UIFramework;
using LiteNetLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Components.Networking;
using Multiplayer.Networking.Packets.Clientbound;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Serverbound;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Networking.Listeners;

public class NetworkClient : NetworkManager
{
    private NetPeer serverPeer;
    // One way ping in milliseconds
    private int ping;
    private readonly ClientPlayerManager playerManager;

    public NetworkClient(Settings settings) : base(settings)
    {
        playerManager = new ClientPlayerManager();
    }

    public void Start(string address, int port, string password)
    {
        netManager.Start();
        ServerboundClientLoginPacket serverboundClientLoginPacket = new() {
            Username = Multiplayer.Settings.Username,
            Password = password,
            BuildMajorVersion = (ushort)BuildInfo.BUILD_VERSION_MAJOR,
            Mods = ModInfo.FromModEntries(UnityModManager.modEntries)
        };
        netPacketProcessor.Write(cachedWriter, serverboundClientLoginPacket);
        netManager.Connect(address, port, cachedWriter);
    }

    protected override void Subscribe()
    {
        netPacketProcessor.SubscribeReusable<ClientboundServerDenyPacket>(OnClientboundServerDenyPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerJoinedPacket>(OnClientboundPlayerJoinedPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerDisconnectPacket>(OnClientboundPlayerDisconnectPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerPositionPacket>(OnClientboundPlayerPositionPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPingUpdatePacket>(OnClientboundPingUpdatePacket);
    }

    #region Common

    public override void OnPeerConnected(NetPeer peer)
    {
        serverPeer = peer;
        if (NetworkLifecycle.Instance.IsHost)
        {
            SendReadyPacket();
            return;
        }

        SceneSwitcher.SwitchToScene(DVScenes.Game);
        NetworkLifecycle.Instance.StartCoroutine(WaitForWorldToLoad());
    }

    private IEnumerator WaitForWorldToLoad()
    {
        while (WorldMover.Instance == null || WorldMover.Instance.originShiftParent == null)
            yield return null;
        yield return null;
        SendReadyPacket();
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        NetworkLifecycle.Instance.Stop();

        if (MainMenuThingsAndStuff.Instance != null)
        {
            MainMenuThingsAndStuff.Instance.SwitchToDefaultMenu();
            NetworkLifecycle.Instance.TriggerMainMenuEventLater();
        }
        else
        {
            MainMenu.GoBackToMainMenu();
        }

        string text = $"{disconnectInfo.Reason}";

        switch (disconnectInfo.Reason)
        {
            case DisconnectReason.DisconnectPeerCalled:
            case DisconnectReason.ConnectionRejected:
                netPacketProcessor.ReadAllPackets(disconnectInfo.AdditionalData);
                return;
            case DisconnectReason.RemoteConnectionClose:
                text = "The server shut down";
                break;
        }

        NetworkLifecycle.Instance.QueueMainMenuEvent(() =>
        {
            Popup popup = MainMenuThingsAndStuff.Instance.ShowOkPopup();
            if (popup == null)
                return;
            popup.labelTMPro.text = text;
        });
    }

    public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        // todo
    }

    public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        ping = latency;
    }

    public override void OnConnectionRequest(ConnectionRequest request)
    {
        // todo
    }

    #endregion

    #region Listeners

    private void OnClientboundServerDenyPacket(ClientboundServerDenyPacket packet)
    {
        NetworkLifecycle.Instance.QueueMainMenuEvent(() =>
        {
            Popup popup = MainMenuThingsAndStuff.Instance.ShowOkPopup();
            if (popup == null)
                return;
            string text = $"{packet.Reason}";
            if (packet.Extra.Length != 0 || packet.Missing.Length != 0)
                text += $"\n\nMissing mods:\n{string.Join("\n - ", packet.Missing)}\n\nExtra mods:\n{string.Join("\n - ", packet.Extra)}";
            popup.labelTMPro.text = text;
        });
    }

    private void OnClientboundPlayerJoinedPacket(ClientboundPlayerJoinedPacket packet)
    {
        Log($"Received player joined packet (Id: {packet.Id}, Username: {packet.Username})");
        playerManager.AddPlayer(packet.Id, packet.Username);
    }

    private void OnClientboundPlayerDisconnectPacket(ClientboundPlayerDisconnectPacket packet)
    {
        Log($"Received player disconnect packet (Id: {packet.Id})");
        playerManager.RemovePlayer(packet.Id);
    }

    private void OnClientboundPlayerPositionPacket(ClientboundPlayerPositionPacket packet)
    {
        playerManager.UpdatePosition(packet);
    }

    private void OnClientboundPingUpdatePacket(ClientboundPingUpdatePacket packet)
    {
        // todo
    }

    #endregion

    #region Senders

    private void SendReadyPacket()
    {
        SendPacket(serverPeer, new ServerboundClientReadyPacket(), DeliveryMethod.ReliableUnordered);
    }

    public void SendPlayerPosition(Vector3 position, float rotationY, bool IsJumping, bool reliable = false)
    {
        SendPacket(serverPeer, new ServerboundPlayerPositionPacket {
            Position = position,
            RotationY = rotationY,
            IsJumping = IsJumping
        }, reliable ? DeliveryMethod.ReliableSequenced : DeliveryMethod.Sequenced);
    }

    #endregion

    #region Logging

    private static void Log(object msg)
    {
        Multiplayer.Log($"[Client] {msg}");
    }

    #endregion
}
