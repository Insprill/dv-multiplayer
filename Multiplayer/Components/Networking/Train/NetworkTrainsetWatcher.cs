using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Networking.Data;
using Multiplayer.Networking.Packets.Clientbound.Train;
using Multiplayer.Utils;

namespace Multiplayer.Components.Networking.Train;

public class NetworkTrainsetWatcher : SingletonBehaviour<NetworkTrainsetWatcher>
{
    private ClientboundTrainsetPhysicsPacket cachedSendPacket;

    protected override void Awake()
    {
        base.Awake();
        if (!NetworkLifecycle.Instance.IsHost())
            return;
        cachedSendPacket = new ClientboundTrainsetPhysicsPacket();
        NetworkLifecycle.Instance.OnTick += Server_OnTick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.OnTick -= Server_OnTick;
    }

    #region Server

    private void Server_OnTick(uint tick)
    {
        cachedSendPacket.Tick = tick;
        foreach (Trainset set in Trainset.allSets)
            Server_TickSet(set);
    }

    private void Server_TickSet(Trainset set)
    {
        bool dirty = false;
        foreach (TrainCar trainCar in set.cars)
        {
            if (trainCar.isStationary)
                continue;
            dirty = true;
            break;
        }

        if (!dirty)
            return;

        cachedSendPacket.NetId = set.firstCar.GetNetId();
        if (cachedSendPacket.TrainsetParts == null || cachedSendPacket.TrainsetParts.Length != set.cars.Count)
            cachedSendPacket.TrainsetParts = new TrainsetPart[set.cars.Count];

        bool anyTracksDirty = false;
        for (int i = 0; i < set.cars.Count; i++)
        {
            TrainCar trainCar = set.cars[i];
            TrainComponentLookup.Instance.NetworkedTrainFromTrain(trainCar, out NetworkedTrainCar networkedTrainCar);
            TrainsetPart trainsetPart = cachedSendPacket.TrainsetParts[i];
            anyTracksDirty |= networkedTrainCar.BogieTracksDirty;
            trainsetPart.Speed = trainCar.GetForwardSpeed();
            trainsetPart.Bogie1 = BogieMovementData.FromBogie(trainCar.Bogies[0], networkedTrainCar.BogieTracksDirty, networkedTrainCar.Bogie1TrackDirection);
            trainsetPart.Bogie1 = BogieMovementData.FromBogie(trainCar.Bogies[1], networkedTrainCar.BogieTracksDirty, networkedTrainCar.Bogie2TrackDirection);
        }

        NetworkLifecycle.Instance.Server.SendTrainsetPhysicsUpdate(cachedSendPacket, anyTracksDirty);
    }

    #endregion

    #region Client

    public void Client_HandleTrainsetPhysicsUpdate(ClientboundTrainsetPhysicsPacket packet)
    {
        Trainset set = Trainset.allSets.Find(set => set.firstCar.GetNetId() == packet.NetId || set.lastCar.GetNetId() == packet.NetId);
        if (set == null)
        {
            Multiplayer.LogDebug(() => $"Received {nameof(ClientboundTrainsetPhysicsPacket)} for unknown trainset with netId {packet.NetId}");
            return;
        }

        if (set.cars.Count != packet.TrainsetParts.Length)
        {
            Multiplayer.LogDebug(() =>
                $"Received {nameof(ClientboundTrainsetPhysicsPacket)} for trainset with netId {packet.NetId} with {packet.TrainsetParts.Length} parts, but trainset has {set.cars.Count} parts");
            return;
        }

        for (int i = 0; i < packet.TrainsetParts.Length; i++)
        {
            TrainComponentLookup.Instance.NetworkedTrainFromTrain(set.cars[i], out NetworkedTrainCar networkedTrainCar);
            networkedTrainCar.Client_ReceiveTrainPhysicsUpdate(packet.TrainsetParts[i], packet.Tick);
        }
    }

    #endregion

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(NetworkTrainsetWatcher)}]";
    }
}
