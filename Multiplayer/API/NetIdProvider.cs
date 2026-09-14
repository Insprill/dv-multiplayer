using DV.CabControls;
using DV.Customization.Paint;
using DV.Logic.Job;
using DV.ThingTypes;
using DV.Utils;
using JetBrains.Annotations;
using MPAPI.Interfaces;
using Multiplayer.Components.Networking.Jobs;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Components.Networking.World;
using System;
using System.Collections.Generic;

namespace Multiplayer.API;

public delegate bool TryGetNetIdDelegate<T>(T obj, out ushort netId) where T : class;
public delegate bool TryGetObjectDelegate<T>(ushort netId, out T obj) where T : class;

public delegate bool TryGetUIntNetIdDelegate<T>(T obj, out uint netId) where T : class;
public delegate bool TryGetObjectUIntDelegate<T>(uint netId, out T obj) where T : class;

internal class NetIdProvider : SingletonBehaviour<NetIdProvider>, INetIdProvider
{
    private readonly Dictionary<Type, object> handlers = [];

    protected override void Awake()
    {
        base.Awake();
        RegisterHandler<TrainCar>(NetworkedTrainCar.TryGetNetId, NetworkedTrainCar.TryGet);
        RegisterHandler<Car>(NetworkedTrainCar.TryGetNetId, NetworkedTrainCar.TryGet);

        RegisterHandler<CargoType_v2>(CargoTypeLookup.Instance.TryGetNetId, CargoTypeLookup.Instance.TryGet);
        RegisterHandler<PaintTheme>(PaintThemeLookup.Instance.TryGetNetId, PaintThemeLookup.Instance.TryGet);

        RegisterHandler<Junction>(NetworkedJunction.TryGetNetId, NetworkedJunction.TryGet);
        RegisterHandler<TurntableRailTrack>(NetworkedTurntable.TryGetNetId, NetworkedTurntable.TryGet);
        RegisterHandler<RailTrack>(NetworkedRailTrack.TryGetNetId, NetworkedRailTrack.TryGet);

        RegisterHandler<StationController>(NetworkedStationController.TryGetNetId, NetworkedStationController.TryGet);
        RegisterHandler<Station>(NetworkedStationController.TryGetNetId, NetworkedStationController.TryGet);
        RegisterHandler<JobValidator>(NetworkedStationController.TryGetNetId, NetworkedStationController.TryGet);

        RegisterHandler<WarehouseMachine>(WarehouseMachineLookup.TryGetNetId, WarehouseMachineLookup.TryGet);
        RegisterHandler<WarehouseMachineController>(NetworkedWarehouseMachineController.TryGetNetId, NetworkedWarehouseMachineController.TryGet);

        RegisterHandler<Job>(NetworkedJob.TryGetNetId, NetworkedJob.TryGetJob);
        RegisterHandler<Task>(NetworkedTask.TryGetNetId, NetworkedTask.TryGet);

        RegisterHandler<ItemBase>(NetworkedItem.TryGetNetId, NetworkedItem.GetItem);
    }

    public void RegisterHandler<T>(TryGetNetIdDelegate<T> tryGetNetId, TryGetObjectDelegate<T> tryGetObject) where T : class
    {
        handlers[typeof(T)] = (tryGetNetId, tryGetObject);
    }

    public void RegisterHandler<T>(TryGetUIntNetIdDelegate<T> tryGetNetId, TryGetObjectUIntDelegate <T> tryGetObject) where T : class
    {
        handlers[typeof(T)] = (tryGetNetId, tryGetObject);
    }

    public bool TryGetNetId<T>(T obj, out ushort netId) where T : class
    {
        netId = 0;

        if (obj == null)
            return false;

        if (handlers.TryGetValue(typeof(T), out var handler) && handler is (TryGetNetIdDelegate<T> tryGetNetId, TryGetObjectDelegate<T> _))
            return tryGetNetId(obj, out netId);

        return false;
    }

    public bool TryGetObject<T>(ushort netId, out T obj) where T : class
    {
        obj = null;

        if (netId == 0)
            return false;

        if (handlers.TryGetValue(typeof(T), out var handler) && handler is (TryGetNetIdDelegate<T> _, TryGetObjectDelegate<T> tryGetObject))
            return tryGetObject(netId, out obj);

        return false;
    }

    public bool TryGetNetId<T>(T obj, out uint netId) where T : class
    {
        netId = 0;

        if (obj == null)
            return false;

        if (handlers.TryGetValue(typeof(T), out var handler) && handler is (TryGetUIntNetIdDelegate<T> tryGetNetId, TryGetObjectUIntDelegate<T> _))
            return tryGetNetId(obj, out netId);

        return false;
    }

    public bool TryGetObject<T>(uint netId, out T obj) where T : class
    {
        obj = null;

        if (netId == 0)
            return false;

        if (handlers.TryGetValue(typeof(T), out var handler) && handler is (TryGetUIntNetIdDelegate<T> _, TryGetObjectUIntDelegate<T> tryGetObject))
            return tryGetObject(netId, out obj);

        return false;
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(NetIdProvider)}]";
    }
}
