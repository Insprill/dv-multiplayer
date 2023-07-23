using System;
using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class NetworkedTrainCar : MonoBehaviour
{
    private static ushort NextNetId = 1;

    [NonSerialized]
    public ushort NetId;

    private TrainCar trainCar;

    private void Awake()
    {
        trainCar = GetComponent<TrainCar>();
        if (NetworkLifecycle.Instance.IsHost())
            NetId = NextNetId++;
    }

    private void Start()
    {
        if (!NetworkLifecycle.Instance.IsHost())
            return;
        NetworkLifecycle.Instance.OnTick += SendBogieUpdate;
    }

    private void OnDestroy()
    {
        if (UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.OnTick -= SendBogieUpdate;
    }

    private void SendBogieUpdate()
    {
        foreach (Bogie bogie in trainCar.Bogies)
            if (!bogie.fullyInitialized || bogie.HasDerailed || bogie.rb == null || bogie.rb.IsSleeping() || bogie.rb.isKinematic)
                return;

        NetworkLifecycle.Instance.Server.SendPhysicsUpdate(trainCar);
    }
}
