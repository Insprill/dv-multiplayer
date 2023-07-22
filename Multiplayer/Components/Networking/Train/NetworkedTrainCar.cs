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

    private void SendBogieUpdate()
    {
        NetworkLifecycle.Instance.Server.SendBogieUpdate(trainCar);
    }
}
