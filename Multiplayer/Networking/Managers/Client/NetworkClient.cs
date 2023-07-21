using System.Net;
using DV;
using DV.UI;
using DV.UIFramework;
using DV.WeatherSystem;
using LiteNetLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Components.Networking;
using Multiplayer.Networking.Packets.Clientbound;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Serverbound;
using Multiplayer.Patches.World;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Networking.Listeners;

public class NetworkClient : NetworkManager
{
    public NetPeer selfPeer { get; private set; }

    // One way ping in milliseconds
    private int ping;
    private NetPeer serverPeer;
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
        selfPeer = netManager.Connect(address, port, cachedWriter);
    }

    protected override void Subscribe()
    {
        netPacketProcessor.SubscribeReusable<ClientboundServerDenyPacket>(OnClientboundServerDenyPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerJoinedPacket>(OnClientboundPlayerJoinedPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerDisconnectPacket>(OnClientboundPlayerDisconnectPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPlayerPositionPacket>(OnClientboundPlayerPositionPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPingUpdatePacket>(OnClientboundPingUpdatePacket);
        netPacketProcessor.SubscribeReusable<ClientboundBeginWorldSyncPacket>(OnClientboundBeginWorldSyncPacket);
        netPacketProcessor.SubscribeReusable<ClientboundWeatherPacket>(OnClientboundWeatherPacket);
        netPacketProcessor.SubscribeReusable<ClientboundRemoveLoadingScreenPacket>(OnClientboundRemoveLoadingScreen);
        netPacketProcessor.SubscribeReusable<ClientboundTimeAdvancePacket>(OnClientboundTimeAdvancePacket);
        netPacketProcessor.SubscribeReusable<ClientboundJunctionStatePacket>(OnClientboundJunctionStatePacket);
        netPacketProcessor.SubscribeReusable<CommonChangeJunctionPacket>(OnCommonChangeJunctionPacket);
    }

    #region Common

    public override void OnPeerConnected(NetPeer peer)
    {
        serverPeer = peer;
        if (NetworkLifecycle.Instance.IsHost(peer))
        {
            SendReadyPacket();
            return;
        }

        SceneSwitcher.SwitchToScene(DVScenes.Game);
        WorldStreamingInit.LoadingFinished += SendReadyPacket;
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
        // todo: update ping in ui
    }

    private void OnClientboundBeginWorldSyncPacket(ClientboundBeginWorldSyncPacket packet)
    {
        Log("Syncing world state");

        DisplayLoadingInfo displayLoadingInfo = Object.FindObjectOfType<DisplayLoadingInfo>();
        if (displayLoadingInfo == null)
        {
            LogError($"Received {nameof(ClientboundBeginWorldSyncPacket)} but couldn't find {nameof(DisplayLoadingInfo)}!");
            return;
        }

        displayLoadingInfo.OnLoadingStatusChanged("Syncing world state", false, 100);
    }

    private void OnClientboundWeatherPacket(ClientboundWeatherPacket packet)
    {
        WeatherDriver.Instance.LoadSaveData(JObject.FromObject(packet));
    }

    private void OnClientboundRemoveLoadingScreen(ClientboundRemoveLoadingScreenPacket packet)
    {
        Log("World sync finished, removing loading screen");

        DisplayLoadingInfo displayLoadingInfo = Object.FindObjectOfType<DisplayLoadingInfo>();
        if (displayLoadingInfo == null)
        {
            LogError($"Received {nameof(ClientboundRemoveLoadingScreenPacket)} but couldn't find {nameof(DisplayLoadingInfo)}!");
            return;
        }

        displayLoadingInfo.OnLoadingFinished();
    }

    private void OnClientboundTimeAdvancePacket(ClientboundTimeAdvancePacket packet)
    {
        TimeAdvance_AdvanceTime_Patch.DontSend = true;
        TimeAdvance.AdvanceTime(packet.amountOfTimeToSkipInSeconds);
        TimeAdvance_AdvanceTime_Patch.DontSend = false;
    }

    private void OnClientboundJunctionStatePacket(ClientboundJunctionStatePacket packet)
    {
        Junction[] junctions = WorldData.Instance.OrderedJunctions;
        for (int i = 0; i < packet.selectedBranches.Length; i++) junctions[i].selectedBranch = packet.selectedBranches[i];
    }

    public void OnCommonChangeJunctionPacket(CommonChangeJunctionPacket packet)
    {
        Junction_Switched_Patch.DontSend = true;
        Junction junction = WorldData.Instance.OrderedJunctions[packet.junctionId];
        junction.selectedBranch = packet.selectedBranch - 1;
        junction.Switch(Junction.SwitchMode.REGULAR);
        Junction_Switched_Patch.DontSend = false;
    }

    #endregion

    #region Senders

    private void SendReadyPacket()
    {
        Log("World loaded, sending ready packet");
        SendPacket(serverPeer, new ServerboundClientReadyPacket(), DeliveryMethod.ReliableOrdered);
    }

    public void SendPlayerPosition(Vector3 position, float rotationY, bool IsJumping, bool reliable = false)
    {
        SendPacket(serverPeer, new ServerboundPlayerPositionPacket {
            Position = position,
            RotationY = rotationY,
            IsJumping = IsJumping
        }, reliable ? DeliveryMethod.ReliableSequenced : DeliveryMethod.Sequenced);
    }

    public void SendTimeAdvance(float amountOfTimeToSkipInSeconds)
    {
        SendPacket(serverPeer, new ServerboundTimeAdvancePacket {
            amountOfTimeToSkipInSeconds = amountOfTimeToSkipInSeconds
        }, DeliveryMethod.ReliableUnordered);
    }

    public void SendJunctionSwitched(ushort junctionId, byte selectedBranch)
    {
        SendPacket(serverPeer, new CommonChangeJunctionPacket {
            junctionId = junctionId,
            selectedBranch = selectedBranch
        }, DeliveryMethod.ReliableUnordered);
    }

    #endregion

    #region Logging

    private static void Log(object msg)
    {
        Multiplayer.Log($"[Client] {msg}");
    }

    private static void LogWarning(object msg)
    {
        Multiplayer.LogWarning($"[Client] {msg}");
    }

    private static void LogError(object msg)
    {
        Multiplayer.LogError($"[Client] {msg}");
    }

    #endregion
}
