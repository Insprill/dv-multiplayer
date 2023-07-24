using System;
using System.Collections;
using DV.Simulation.Brake;
using Multiplayer.Components.Networking.World;
using Multiplayer.Networking.Packets.Clientbound.Train;
using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class NetworkedTrainCar : MonoBehaviour
{
    private static ushort NextNetId = 1;

    [NonSerialized]
    public ushort NetId;

    private TrainCar trainCar;
    private Bogie bogie1;
    private Bogie bogie2;
    private BrakeSystem brakeSystem;

    private bool handbrakeDirty;

    #region Client

    private bool client_Initialized;
    private NetworkedRigidbody client_trainCarNetworkedRigidbody;
    private NetworkedRigidbody client_bogie1NetworkedRigidbody;
    private NetworkedRigidbody client_bogie2NetworkedRigidbody;

    #endregion

    private void Awake()
    {
        trainCar = GetComponent<TrainCar>();
        bogie1 = trainCar.Bogies[0];
        bogie2 = trainCar.Bogies[1];
        brakeSystem = trainCar.brakeSystem;
        if (NetworkLifecycle.Instance.IsHost())
            NetId = NextNetId++;
        else
            StartCoroutine(WaitForPhysicSetup());
    }

    private void OnEnable()
    {
        brakeSystem.HandbrakePositionChanged += OnHandbrakePositionChanged;
        brakeSystem.BrakeCylinderReleased += OnBrakeCylinderReleased;
        NetworkLifecycle.Instance.OnTick += Common_OnTick;
        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.OnTick += Server_OnTick;
    }

    private void OnDisable()
    {
        if (UnloadWatcher.isQuitting)
            return;
        brakeSystem.HandbrakePositionChanged -= OnHandbrakePositionChanged;
        brakeSystem.BrakeCylinderReleased -= OnBrakeCylinderReleased;
        NetworkLifecycle.Instance.OnTick -= Common_OnTick;
        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.OnTick -= Server_OnTick;
    }

    #region Server

    private void Server_OnTick()
    {
        Server_SendPhysicsUpdate();
    }

    private void Server_SendPhysicsUpdate()
    {
        if (trainCar.isStationary || (!ShouldSendBogie(bogie1) && !ShouldSendBogie(bogie2)))
            return;
        NetworkLifecycle.Instance.Server.SendPhysicsUpdate(trainCar);
    }

    private static bool ShouldSendBogie(Bogie bogie)
    {
        return bogie.fullyInitialized && !bogie.HasDerailed && bogie.rb != null && !bogie.rb.IsSleeping() && !bogie.rb.isKinematic;
    }

    #endregion

    #region Common

    private void Common_OnTick()
    {
        if (!trainCar.brakeSystem.hasHandbrake || !handbrakeDirty)
            return;
        NetworkLifecycle.Instance.Client.SendHandbrakePositionChanged(trainCar);
    }

    private void OnHandbrakePositionChanged((float, bool) data)
    {
        handbrakeDirty = !NetworkLifecycle.Instance.IsProcessingPacket;
    }

    private void OnBrakeCylinderReleased()
    {
        NetworkLifecycle.Instance.Client.SendBrakeCylinderReleased(trainCar);
    }

    #endregion

    #region Client

    private IEnumerator WaitForPhysicSetup()
    {
        while ((client_trainCarNetworkedRigidbody = trainCar.GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        while ((client_bogie1NetworkedRigidbody = trainCar.Bogies[0].GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        while ((client_bogie2NetworkedRigidbody = trainCar.Bogies[1].GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        client_Initialized = true;
    }

    public void Client_ReceiveTrainPhysicsUpdate(ClientboundTrainPhysicsPacket packet)
    {
        if (!client_Initialized)
            return;
        trainCar.ForceOptimizationState(false);
        client_trainCarNetworkedRigidbody.ReceiveSnapshot(packet.Car, packet.Timestamp);
        client_bogie1NetworkedRigidbody.ReceiveSnapshot(packet.Bogie1, packet.Timestamp);
        client_bogie2NetworkedRigidbody.ReceiveSnapshot(packet.Bogie2, packet.Timestamp);
    }

    #endregion
}
