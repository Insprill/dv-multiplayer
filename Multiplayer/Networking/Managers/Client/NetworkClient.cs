using System.Collections.Generic;
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
        netPacketProcessor.SubscribeReusable<ClientboundTurntableStatePacket>(OnClientboundTurntableStatePacket);
        netPacketProcessor.SubscribeReusable<CommonChangeJunctionPacket>(OnCommonChangeJunctionPacket);
        netPacketProcessor.SubscribeReusable<CommonRotateTurntablePacket>(OnCommonRotateTurntablePacket);
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
        for (int i = 0; i < packet.selectedBranches.Length; i++)
            junctions[i].selectedBranch = packet.selectedBranches[i];
    }

    private void OnClientboundTurntableStatePacket(ClientboundTurntableStatePacket packet)
    {
        for (int i = 0; i < packet.rotations.Length; i++)
        {
            TurntableRailTrack turntableRailTrack = TurntableController.allControllers[i].turntable;
            turntableRailTrack.targetYRotation = packet.rotations[i];
            turntableRailTrack.RotateToTargetRotation(true);
        }
    }

    private void OnCommonChangeJunctionPacket(CommonChangeJunctionPacket packet)
    {
        Junction[] orderedJunctions = WorldData.Instance.OrderedJunctions;
        if (packet.Index >= orderedJunctions.Length)
        {
            LogWarning($"Received {nameof(CommonChangeJunctionPacket)} for junction with index {packet.Index}, but there's only {orderedJunctions.Length} junctions on the map!");
            return;
        }

        Junction junction = orderedJunctions[packet.Index];
        Junction_Switched_Patch.DontSend = true;
        junction.selectedBranch = packet.SelectedBranch - 1; // Junction#Switch increments this before processing
        junction.Switch((Junction.SwitchMode)packet.Mode);
        Junction_Switched_Patch.DontSend = false;
    }

    private void OnCommonRotateTurntablePacket(CommonRotateTurntablePacket packet)
    {
        List<TurntableController> controllers = TurntableController.allControllers;
        if (packet.index >= controllers.Count)
        {
            LogWarning($"Received {nameof(CommonRotateTurntablePacket)} for turntable with index {packet.index}, but there's only {controllers.Count} turntables on the map!");
            return;
        }

        TurntableRailTrack turntable = controllers[packet.index].turntable;
        turntable.targetYRotation = packet.rotation;

        TurntableRailTrack_RotateToTargetRotation_Patch.DontSend = true;
        turntable.RotateToTargetRotation();
        TurntableRailTrack_RotateToTargetRotation_Patch.DontSend = false;
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

    public void SendJunctionSwitched(ushort index, byte selectedBranch, Junction.SwitchMode mode)
    {
        SendPacket(serverPeer, new CommonChangeJunctionPacket {
            Index = index,
            SelectedBranch = selectedBranch,
            Mode = (byte)mode
        }, DeliveryMethod.ReliableUnordered);
    }

    public void SendTurntableRotation(byte index, float rotation)
    {
        SendPacket(serverPeer, new CommonRotateTurntablePacket {
            index = index,
            rotation = rotation
        }, DeliveryMethod.ReliableOrdered);
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
