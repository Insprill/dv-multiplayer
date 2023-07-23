using System;
using System.Collections;
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

    private bool clientInitialized;
    private NetworkedRigidbody trainCarNetworkedRigidbody;
    private NetworkedRigidbody bogie1NetworkedRigidbody;
    private NetworkedRigidbody bogie2NetworkedRigidbody;

    private void Awake()
    {
        trainCar = GetComponent<TrainCar>();
        bogie1 = trainCar.Bogies[0];
        bogie2 = trainCar.Bogies[1];
        if (NetworkLifecycle.Instance.IsHost())
            NetId = NextNetId++;
        else
            StartCoroutine(WaitForPhysicSetup());
    }

    private void OnEnable()
    {
        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.OnTick += Server_OnTick;
    }

    private void OnDisable()
    {
        if (UnloadWatcher.isQuitting)
            return;
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

    #region Client

    private IEnumerator WaitForPhysicSetup()
    {
        while ((trainCarNetworkedRigidbody = trainCar.GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        while ((bogie1NetworkedRigidbody = trainCar.Bogies[0].GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        while ((bogie2NetworkedRigidbody = trainCar.Bogies[1].GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        clientInitialized = true;
    }

    public void Client_ReceiveTrainPhysicsUpdate(ClientboundTrainPhysicsPacket packet)
    {
        if (!clientInitialized)
            return;
        trainCar.ForceOptimizationState(false);
        trainCarNetworkedRigidbody.ReceiveSnapshot(packet.Car, packet.Timestamp);
        bogie1NetworkedRigidbody.ReceiveSnapshot(packet.Bogie1, packet.Timestamp);
        bogie2NetworkedRigidbody.ReceiveSnapshot(packet.Bogie2, packet.Timestamp);
    }

    #endregion
}
