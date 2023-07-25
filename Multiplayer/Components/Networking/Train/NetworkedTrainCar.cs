using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DV.Simulation.Brake;
using DV.Simulation.Cars;
using LocoSim.Definitions;
using LocoSim.Implementations;
using Multiplayer.Components.Networking.World;
using Multiplayer.Networking.Packets.Clientbound.Train;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Packets.Common.Train;
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

    private bool hasSimFlow;
    private SimulationFlow simulationFlow;
    private HashSet<string> dirtyPorts;
    private HashSet<string> dirtyFuses;
    private bool handbrakeDirty;

    #region Client

    private bool client_Initialized;
    private TickedQueue<RigidbodySnapshot> client_trainCarNetworkedRigidbody;
    private TickedQueue<BogieMovementData> client_bogie1Queue;
    private TickedQueue<BogieMovementData> client_bogie2Queue;

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

        SimController simController = GetComponent<SimController>();
        if (simController == null)
            return;

        hasSimFlow = true;
        simulationFlow = simController.SimulationFlow;

        dirtyPorts = new HashSet<string>(simulationFlow.fullPortIdToPort.Count);
        foreach (KeyValuePair<string, Port> kvp in simulationFlow.fullPortIdToPort)
            if (kvp.Value.valueType == PortValueType.CONTROL || NetworkLifecycle.Instance.IsHost())
                kvp.Value.ValueUpdatedInternally += _ => { Common_OnPortUpdated(kvp.Key); }; // todo: secure this

        dirtyFuses = new HashSet<string>(simulationFlow.fullFuseIdToFuse.Count);
        foreach (KeyValuePair<string, Fuse> kvp in simulationFlow.fullFuseIdToFuse)
            kvp.Value.StateUpdated += _ => { Common_OnFuseUpdated(kvp.Key); };
    }

    private void OnEnable()
    {
        brakeSystem.HandbrakePositionChanged += Common_OnHandbrakePositionChanged;
        brakeSystem.BrakeCylinderReleased += Common_OnBrakeCylinderReleased;
        NetworkLifecycle.Instance.OnTick += Common_OnTick;
        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.OnTick += Server_OnTick;
    }

    private void OnDisable()
    {
        if (UnloadWatcher.isQuitting)
            return;
        brakeSystem.HandbrakePositionChanged -= Common_OnHandbrakePositionChanged;
        brakeSystem.BrakeCylinderReleased -= Common_OnBrakeCylinderReleased;
        NetworkLifecycle.Instance.OnTick -= Common_OnTick;
        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.OnTick -= Server_OnTick;
    }

    #region Server

    public void Server_DirtyAllState()
    {
        handbrakeDirty = true;
        if (!hasSimFlow)
            return;
        foreach (string portId in simulationFlow.fullPortIdToPort.Keys)
            dirtyPorts.Add(portId);
        foreach (string fuseId in simulationFlow.fullFuseIdToFuse.Keys)
            dirtyFuses.Add(fuseId);
    }

    private void Server_OnTick()
    {
        Server_SendPhysicsUpdate();
    }

    private void Server_SendPhysicsUpdate()
    {
        if (trainCar.isStationary || !bogie1.fullyInitialized || bogie1.rb == null || !bogie2.fullyInitialized || bogie2.rb == null)
            return;
        NetworkLifecycle.Instance.Server.SendPhysicsUpdate(trainCar, NetId, bogie1, bogie2);
    }

    #endregion

    #region Common

    private void Common_OnTick()
    {
        Common_SendHandbrakePosition();
        Common_SendSimFlow();
    }

    private void Common_SendHandbrakePosition()
    {
        if (!handbrakeDirty)
            return;
        if (!trainCar.brakeSystem.hasHandbrake)
            return;
        handbrakeDirty = false;
        NetworkLifecycle.Instance.Client.SendHandbrakePositionChanged(trainCar);
    }

    private void Common_SendSimFlow()
    {
        if (!hasSimFlow)
            return;
        if (dirtyPorts.Count == 0 && dirtyFuses.Count == 0)
            return;

        int i = 0;
        string[] portIds = dirtyPorts.ToArray();
        float[] portValues = new float[portIds.Length];
        foreach (string portId in dirtyPorts) portValues[i++] = simulationFlow.fullPortIdToPort[portId].Value;

        i = 0;
        string[] fuseIds = dirtyFuses.ToArray();
        bool[] fuseValues = new bool[fuseIds.Length];
        foreach (string fuseId in dirtyFuses) fuseValues[i++] = simulationFlow.fullFuseIdToFuse[fuseId].State;

        dirtyPorts.Clear();
        dirtyFuses.Clear();

        NetworkLifecycle.Instance.Client.SendSimFlow(NetId, portIds, portValues, fuseIds, fuseValues);
    }

    private void Common_OnHandbrakePositionChanged((float, bool) data)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        handbrakeDirty = true;
    }

    private void Common_OnBrakeCylinderReleased()
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        NetworkLifecycle.Instance.Client.SendBrakeCylinderReleased(trainCar);
    }

    private void Common_OnPortUpdated(string portId)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        dirtyPorts.Add(portId);
    }

    private void Common_OnFuseUpdated(string portId)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        dirtyFuses.Add(portId);
    }

    public void Common_UpdateSimFlow(CommonSimFlowPacket packet)
    {
        for (int i = 0; i < packet.PortIds.Length; i++)
        {
            Port port = simulationFlow.fullPortIdToPort[packet.PortIds[i]];
            float value = packet.PortValues[i];
            if (port.type == PortType.EXTERNAL_IN)
                port.ExternalValueUpdate(value);
            else
                port.Value = value;
        }

        for (int i = 0; i < packet.FuseIds.Length; i++)
            simulationFlow.fullFuseIdToFuse[packet.FuseIds[i]].ChangeState(packet.FuseValues[i]);
    }

    #endregion

    #region Client

    private IEnumerator WaitForPhysicSetup()
    {
        while ((client_trainCarNetworkedRigidbody = trainCar.GetComponent<NetworkedRigidbody>()) == null)
            yield return null;
        while ((client_bogie1Queue = bogie1.GetComponent<NetworkedBogie>()) == null)
            yield return null;
        while ((client_bogie2Queue = bogie2.GetComponent<NetworkedBogie>()) == null)
            yield return null;
        client_Initialized = true;
    }

    public void Client_ReceiveTrainPhysicsUpdate(ClientboundTrainPhysicsPacket packet)
    {
        if (!client_Initialized)
            return;
        if (trainCar.isEligibleForSleep)
            trainCar.ForceOptimizationState(false);
        client_trainCarNetworkedRigidbody.ReceiveSnapshot(packet.Car, packet.Timestamp);
        client_bogie1Queue.ReceiveSnapshot(packet.Bogie1, packet.Timestamp);
        client_bogie2Queue.ReceiveSnapshot(packet.Bogie2, packet.Timestamp);
    }

    #endregion
}
