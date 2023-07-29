using System.Collections.Generic;
using DV;
using DV.Damage;
using DV.Logic.Job;
using DV.MultipleUnit;
using DV.ThingTypes;
using DV.UI;
using DV.UIFramework;
using DV.WeatherSystem;
using LiteNetLib;
using Multiplayer.Components;
using Multiplayer.Components.MainMenu;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Networking.Packets.Clientbound;
using Multiplayer.Networking.Packets.Clientbound.Train;
using Multiplayer.Networking.Packets.Clientbound.World;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Common.Train;
using Multiplayer.Networking.Packets.Serverbound;
using Multiplayer.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Networking.Listeners;

public class NetworkClient : NetworkManager
{
    protected override string LogPrefix => "[Client]";

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
        netPacketProcessor.SubscribeReusable<ClientboundPlayerCarPacket>(OnClientboundPlayerCarPacket);
        netPacketProcessor.SubscribeReusable<ClientboundPingUpdatePacket>(OnClientboundPingUpdatePacket);
        netPacketProcessor.SubscribeReusable<ClientboundTickSyncPacket>(OnClientboundTickSyncPacket);
        netPacketProcessor.SubscribeReusable<ClientboundServerLoadingPacket>(OnClientboundServerLoadingPacket);
        netPacketProcessor.SubscribeReusable<ClientboundBeginWorldSyncPacket>(OnClientboundBeginWorldSyncPacket);
        netPacketProcessor.SubscribeReusable<ClientboundGameParamsPacket>(OnClientboundGameParamsPacket);
        netPacketProcessor.SubscribeReusable<ClientboundWeatherPacket>(OnClientboundWeatherPacket);
        netPacketProcessor.SubscribeReusable<ClientboundRemoveLoadingScreenPacket>(OnClientboundRemoveLoadingScreen);
        netPacketProcessor.SubscribeReusable<ClientboundTimeAdvancePacket>(OnClientboundTimeAdvancePacket);
        netPacketProcessor.SubscribeReusable<ClientboundJunctionStatePacket>(OnClientboundJunctionStatePacket);
        netPacketProcessor.SubscribeReusable<ClientboundTurntableStatePacket>(OnClientboundTurntableStatePacket);
        netPacketProcessor.SubscribeReusable<CommonChangeJunctionPacket>(OnCommonChangeJunctionPacket);
        netPacketProcessor.SubscribeReusable<CommonRotateTurntablePacket>(OnCommonRotateTurntablePacket);
        netPacketProcessor.SubscribeReusable<ClientboundSpawnNewTrainCarPacket>(OnClientboundSpawnNewTrainCarPacket);
        netPacketProcessor.SubscribeReusable<ClientboundSpawnExistingTrainCarPacket>(OnClientboundSpawnExistingTrainCarPacket);
        netPacketProcessor.SubscribeReusable<ClientboundDestroyTrainCarPacket>(OnClientboundDestroyTrainCarPacket);
        netPacketProcessor.SubscribeReusable<ClientboundTrainPhysicsPacket>(OnClientboundTrainRigidbodyPacket);
        netPacketProcessor.SubscribeReusable<CommonTrainCouplePacket>(OnCommonTrainCouplePacket);
        netPacketProcessor.SubscribeReusable<CommonTrainUncouplePacket>(OnCommonTrainUncouplePacket);
        netPacketProcessor.SubscribeReusable<CommonHoseConnectedPacket>(OnCommonHoseConnectedPacket);
        netPacketProcessor.SubscribeReusable<CommonHoseDisconnectedPacket>(OnCommonHoseDisconnectedPacket);
        netPacketProcessor.SubscribeReusable<CommonMuConnectedPacket>(OnCommonMuConnectedPacket);
        netPacketProcessor.SubscribeReusable<CommonMuDisconnectedPacket>(OnCommonMuDisconnectedPacket);
        netPacketProcessor.SubscribeReusable<CommonCockFiddlePacket>(OnCommonCockFiddlePacket);
        netPacketProcessor.SubscribeReusable<CommonBrakeCylinderReleasePacket>(OnCommonBrakeCylinderReleasePacket);
        netPacketProcessor.SubscribeReusable<CommonHandbrakePositionPacket>(OnCommonHandbrakePositionPacket);
        netPacketProcessor.SubscribeReusable<CommonSimFlowPacket>(OnCommonSimFlowPacket);
        netPacketProcessor.SubscribeReusable<ClientboundCargoStatePacket>(OnClientboundCargoStatePacket);
        netPacketProcessor.SubscribeReusable<ClientboundCarHealthUpdatePacket>(OnClientboundCarHealthUpdatePacket);
    }

    #region Net Events

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

        TrainStress.globalIgnoreStressCalculation = true;
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

    private void OnClientboundPlayerCarPacket(ClientboundPlayerCarPacket packet)
    {
        playerManager.UpdateCar(packet.Id, packet.CarId);
    }

    private void OnClientboundPingUpdatePacket(ClientboundPingUpdatePacket packet)
    {
        playerManager.UpdatePing(packet.Id, packet.Ping);
    }

    private void OnClientboundTickSyncPacket(ClientboundTickSyncPacket packet)
    {
        NetworkLifecycle.Instance.Tick = (uint)(packet.ServerTick + ping / 2.0f * (1f / NetworkLifecycle.TICK_RATE));
    }

    private void OnClientboundServerLoadingPacket(ClientboundServerLoadingPacket packet)
    {
        Log("Waiting for server to load");

        DisplayLoadingInfo displayLoadingInfo = Object.FindObjectOfType<DisplayLoadingInfo>();
        if (displayLoadingInfo == null)
        {
            LogError($"Received {nameof(ClientboundServerLoadingPacket)} but couldn't find {nameof(DisplayLoadingInfo)}!");
            return;
        }

        displayLoadingInfo.OnLoadingStatusChanged("Waiting for server to load", false, 100);
    }

    private void OnClientboundGameParamsPacket(ClientboundGameParamsPacket packet)
    {
        Multiplayer.LogDebug(() => $"Received {nameof(ClientboundGameParamsPacket)} ({packet.SerializedGameParams.Length} chars)");
        if (Globals.G.gameParams != null)
            packet.Apply(Globals.G.gameParams);
        if (Globals.G.gameParamsInstance != null)
            packet.Apply(Globals.G.gameParamsInstance);
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
        TimeAdvance.AdvanceTime(packet.amountOfTimeToSkipInSeconds);
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
        junction.selectedBranch = packet.SelectedBranch - 1; // Junction#Switch increments this before processing
        junction.Switch((Junction.SwitchMode)packet.Mode);
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
        turntable.RotateToTargetRotation();
    }

    public void OnClientboundSpawnNewTrainCarPacket(ClientboundSpawnNewTrainCarPacket packet)
    {
        RailTrack track = RailTrackRegistry.Instance.GetTrackWithName(packet.Track);
        if (track == null)
        {
            LogError($"Received {nameof(ClientboundSpawnNewTrainCarPacket)} but couldn't find track with name {packet.Track}");
            return;
        }

        if (!TrainComponentLookup.Instance.LiveryFromId(packet.LiveryId, out TrainCarLivery livery))
        {
            LogError($"Received {nameof(ClientboundSpawnNewTrainCarPacket)} but couldn't find TrainCarLivery with ID {packet.LiveryId}");
            return;
        }

        CarSpawner.Instance.SpawnCar(livery.prefab, track, packet.Position - WorldMover.currentMove, packet.Forward, packet.PlayerSpawnedCar).SetNetId(packet.NetId);
    }

    public void OnClientboundSpawnExistingTrainCarPacket(ClientboundSpawnExistingTrainCarPacket packet)
    {
        RailTrack bogie1Track = RailTrackRegistry.Instance.GetTrackWithName(packet.Bogie1.Track);
        if (!string.IsNullOrEmpty(packet.Bogie1.Track) && bogie1Track == null)
        {
            LogError($"Received {nameof(ClientboundSpawnExistingTrainCarPacket)} but couldn't find track with name {packet.Bogie1.Track}");
            return;
        }

        RailTrack bogie2Track = RailTrackRegistry.Instance.GetTrackWithName(packet.Bogie2.Track);
        if (!string.IsNullOrEmpty(packet.Bogie1.Track) && bogie2Track == null)
        {
            LogError($"Received {nameof(ClientboundSpawnExistingTrainCarPacket)} but couldn't find track with name {packet.Bogie2.Track}");
            return;
        }

        if (!TrainComponentLookup.Instance.LiveryFromId(packet.LiveryId, out TrainCarLivery livery))
        {
            LogError($"Received {nameof(ClientboundSpawnExistingTrainCarPacket)} but couldn't find TrainCarLivery with ID {packet.LiveryId}");
            return;
        }

        CarSpawner.Instance.SpawnLoadedCar(
            livery.prefab,
            packet.CarId,
            packet.CarGuid,
            packet.PlayerSpawnedCar,
            packet.Position - WorldMover.currentMove,
            Quaternion.Euler(packet.Rotation),
            packet.Bogie1.IsDerailed,
            bogie1Track,
            packet.Bogie1.PositionAlongTrack,
            packet.Bogie2.IsDerailed,
            bogie2Track,
            packet.Bogie2.PositionAlongTrack,
            packet.CouplerFCoupled,
            packet.CouplerRCoupled
        ).SetNetId(packet.NetId);
    }

    public void OnClientboundDestroyTrainCarPacket(ClientboundDestroyTrainCarPacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(ClientboundDestroyTrainCarPacket)} but couldn't find the car!");
            return;
        }

        trainCar.ReturnCarToPool();
    }

    public void OnClientboundTrainRigidbodyPacket(ClientboundTrainPhysicsPacket packet)
    {
        if (!TrainComponentLookup.Instance.NetworkedTrainFromNetId(packet.NetId, out NetworkedTrainCar networkedTrainCar))
            return;

        networkedTrainCar.Client_ReceiveTrainPhysicsUpdate(packet);
    }

    private void OnCommonTrainCouplePacket(CommonTrainCouplePacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar) || !TrainComponentLookup.Instance.TrainFromNetId(packet.OtherNetId, out TrainCar otherTrainCar))
        {
            LogError($"Received {nameof(CommonTrainCouplePacket)} but couldn't find one of the cars!");
            return;
        }

        Coupler coupler = packet.IsFrontCoupler ? trainCar.frontCoupler : trainCar.rearCoupler;
        Coupler otherCoupler = packet.OtherCarIsFrontCoupler ? otherTrainCar.frontCoupler : otherTrainCar.rearCoupler;

        coupler.CoupleTo(otherCoupler, packet.PlayAudio, packet.ViaChainInteraction);
    }

    private void OnCommonTrainUncouplePacket(CommonTrainUncouplePacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(CommonTrainUncouplePacket)} but couldn't find one of the cars!");
            return;
        }

        Coupler coupler = packet.IsFrontCoupler ? trainCar.frontCoupler : trainCar.rearCoupler;

        coupler.Uncouple(packet.PlayAudio, false, packet.DueToBrokenCouple, packet.ViaChainInteraction);
    }

    private void OnCommonHoseConnectedPacket(CommonHoseConnectedPacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar) || !TrainComponentLookup.Instance.TrainFromNetId(packet.OtherNetId, out TrainCar otherTrainCar))
        {
            LogError($"Received {nameof(CommonHoseConnectedPacket)} but couldn't find one of the cars!");
            return;
        }

        Coupler coupler = packet.IsFront ? trainCar.frontCoupler : trainCar.rearCoupler;
        Coupler otherCoupler = packet.OtherIsFront ? otherTrainCar.frontCoupler : otherTrainCar.rearCoupler;

        coupler.ConnectAirHose(otherCoupler, packet.PlayAudio);
    }

    private void OnCommonHoseDisconnectedPacket(CommonHoseDisconnectedPacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(CommonHoseDisconnectedPacket)} but couldn't find one of the cars!");
            return;
        }

        Coupler coupler = packet.IsFront ? trainCar.frontCoupler : trainCar.rearCoupler;

        coupler.DisconnectAirHose(packet.PlayAudio);
    }

    private void OnCommonMuConnectedPacket(CommonMuConnectedPacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar) || !TrainComponentLookup.Instance.TrainFromNetId(packet.OtherNetId, out TrainCar otherTrainCar))
        {
            LogError($"Received {nameof(CommonMuConnectedPacket)} but couldn't find one of the cars!");
            return;
        }

        MultipleUnitCable cable = packet.IsFront ? trainCar.muModule.frontCable : trainCar.muModule.rearCable;
        MultipleUnitCable otherCable = packet.IsFront ? otherTrainCar.muModule.frontCable : otherTrainCar.muModule.rearCable;

        cable.Connect(otherCable, packet.PlayAudio);
    }

    private void OnCommonMuDisconnectedPacket(CommonMuDisconnectedPacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(CommonMuDisconnectedPacket)} but couldn't find one of the cars!");
            return;
        }

        MultipleUnitCable cable = packet.IsFront ? trainCar.muModule.frontCable : trainCar.muModule.rearCable;

        cable.Disconnect(packet.PlayAudio);
    }

    private void OnCommonCockFiddlePacket(CommonCockFiddlePacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(CommonCockFiddlePacket)} but couldn't find one of the cars!");
            return;
        }

        Coupler coupler = packet.IsFront ? trainCar.frontCoupler : trainCar.rearCoupler;

        coupler.IsCockOpen = packet.IsOpen;
    }

    private void OnCommonBrakeCylinderReleasePacket(CommonBrakeCylinderReleasePacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(CommonCockFiddlePacket)} but couldn't find one of the cars!");
            return;
        }

        trainCar.brakeSystem.ReleaseBrakeCylinderPressure();
    }

    private void OnCommonHandbrakePositionPacket(CommonHandbrakePositionPacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(CommonCockFiddlePacket)} but couldn't find one of the cars!");
            return;
        }

        trainCar.brakeSystem.SetHandbrakePosition(packet.Position);
    }

    private void OnCommonSimFlowPacket(CommonSimFlowPacket packet)
    {
        if (!TrainComponentLookup.Instance.NetworkedTrainFromNetId(packet.NetId, out NetworkedTrainCar networkedTrainCar))
        {
            LogError($"Received {nameof(CommonSimFlowPacket)} but couldn't find one of the cars!");
            return;
        }

        networkedTrainCar.Common_UpdateSimFlow(packet);
    }

    private void OnClientboundCargoStatePacket(ClientboundCargoStatePacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(ClientboundCargoStatePacket)} but couldn't find one of the cars!");
            return;
        }

        if (packet.CargoType == (ushort)CargoType.None && trainCar.logicCar.CurrentCargoTypeInCar == CargoType.None)
            return;

        // todo: cache warehouse machine
        WarehouseMachine warehouse = string.IsNullOrEmpty(packet.WarehouseMachineId) ? null : JobSaveManager.Instance.GetWarehouseMachineWithId(packet.WarehouseMachineId);
        if (packet.IsLoading)
            trainCar.logicCar.LoadCargo(packet.CargoAmount, (CargoType)packet.CargoType, warehouse);
        else
            trainCar.logicCar.UnloadCargo(packet.CargoAmount, (CargoType)packet.CargoType, warehouse);
    }

    private void OnClientboundCarHealthUpdatePacket(ClientboundCarHealthUpdatePacket packet)
    {
        if (!TrainComponentLookup.Instance.TrainFromNetId(packet.NetId, out TrainCar trainCar))
        {
            LogError($"Received {nameof(ClientboundCarHealthUpdatePacket)} but couldn't find one of the cars!");
            return;
        }

        CarDamageModel carDamage = trainCar.CarDamage;
        float difference = Mathf.Abs(packet.Health - carDamage.currentHealth);
        if (difference < 0.0001)
            return;

        if (packet.Health < carDamage.currentHealth)
            carDamage.DamageCar(difference);
        else
            carDamage.RepairCar(difference);
    }

    #endregion

    #region Senders

    private void SendPacketToServer<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
    {
        SendPacket(serverPeer, packet, deliveryMethod);
    }

    private void SendReadyPacket()
    {
        Log("World loaded, sending ready packet");
        SendPacketToServer(new ServerboundClientReadyPacket(), DeliveryMethod.ReliableOrdered);
    }

    public void SendPlayerPosition(Vector3 position, Vector3 moveDir, float rotationY, bool isJumping, bool reliable = false)
    {
        SendPacketToServer(new ServerboundPlayerPositionPacket {
            Position = position,
            MoveDir = new Vector2(moveDir.x, moveDir.z),
            RotationY = rotationY,
            IsJumping = isJumping
        }, reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Sequenced);
    }

    public void SendPlayerCar(ushort carId)
    {
        SendPacketToServer(new ServerboundPlayerCarPacket {
            CarId = carId
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendTimeAdvance(float amountOfTimeToSkipInSeconds)
    {
        SendPacketToServer(new ServerboundTimeAdvancePacket {
            amountOfTimeToSkipInSeconds = amountOfTimeToSkipInSeconds
        }, DeliveryMethod.ReliableUnordered);
    }

    public void SendJunctionSwitched(ushort index, byte selectedBranch, Junction.SwitchMode mode)
    {
        SendPacketToServer(new CommonChangeJunctionPacket {
            Index = index,
            SelectedBranch = selectedBranch,
            Mode = (byte)mode
        }, DeliveryMethod.ReliableUnordered);
    }

    public void SendTurntableRotation(byte index, float rotation)
    {
        SendPacketToServer(new CommonRotateTurntablePacket {
            index = index,
            rotation = rotation
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendTrainCouple(Coupler coupler, Coupler otherCoupler, bool playAudio, bool viaChainInteraction)
    {
        SendPacketToServer(new CommonTrainCouplePacket {
            NetId = coupler.train.GetNetId(),
            IsFrontCoupler = coupler.isFrontCoupler,
            OtherNetId = otherCoupler.train.GetNetId(),
            OtherCarIsFrontCoupler = otherCoupler.isFrontCoupler,
            PlayAudio = playAudio,
            ViaChainInteraction = viaChainInteraction
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendTrainUncouple(Coupler coupler, bool playAudio, bool dueToBrokenCouple, bool viaChainInteraction)
    {
        SendPacketToServer(new CommonTrainUncouplePacket {
            NetId = coupler.train.GetNetId(),
            IsFrontCoupler = coupler.isFrontCoupler,
            PlayAudio = playAudio,
            ViaChainInteraction = viaChainInteraction,
            DueToBrokenCouple = dueToBrokenCouple
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendHoseConnected(Coupler coupler, Coupler otherCoupler, bool playAudio)
    {
        SendPacketToServer(new CommonHoseConnectedPacket {
            NetId = coupler.train.GetNetId(),
            IsFront = coupler.isFrontCoupler,
            OtherNetId = otherCoupler.train.GetNetId(),
            OtherIsFront = otherCoupler.isFrontCoupler,
            PlayAudio = playAudio
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendHoseDisconnected(Coupler coupler, bool playAudio)
    {
        SendPacketToServer(new CommonHoseDisconnectedPacket {
            NetId = coupler.train.GetNetId(),
            IsFront = coupler.isFrontCoupler,
            PlayAudio = playAudio
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendMuConnected(MultipleUnitCable cable, MultipleUnitCable otherCable, bool playAudio)
    {
        SendPacketToServer(new CommonMuConnectedPacket {
            NetId = cable.muModule.train.GetNetId(),
            IsFront = cable.isFront,
            OtherNetId = otherCable.muModule.train.GetNetId(),
            OtherIsFront = otherCable.isFront,
            PlayAudio = playAudio
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendMuDisconnected(MultipleUnitCable cable, bool playAudio)
    {
        SendPacketToServer(new CommonMuDisconnectedPacket {
            NetId = cable.muModule.train.GetNetId(),
            IsFront = cable.isFront,
            PlayAudio = playAudio
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendCockState(Coupler coupler, bool isOpen)
    {
        SendPacketToServer(new CommonCockFiddlePacket {
            NetId = coupler.train.GetNetId(),
            IsFront = coupler.isFrontCoupler,
            IsOpen = isOpen
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendBrakeCylinderReleased(TrainCar trainCar)
    {
        SendPacketToServer(new CommonBrakeCylinderReleasePacket {
            NetId = trainCar.GetNetId()
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendHandbrakePositionChanged(TrainCar trainCar)
    {
        SendPacketToServer(new CommonHandbrakePositionPacket {
            NetId = trainCar.GetNetId(),
            Position = trainCar.brakeSystem.handbrakePosition
        }, DeliveryMethod.ReliableOrdered);
    }

    public void SendSimFlow(ushort netId, string[] portIds, float[] portValues, string[] fuseIds, bool[] fuseValues)
    {
        SendPacketToServer(new CommonSimFlowPacket {
            NetId = netId,
            PortIds = portIds,
            PortValues = portValues,
            FuseIds = fuseIds,
            FuseValues = fuseValues
        }, DeliveryMethod.ReliableOrdered);
    }

    #endregion
}
