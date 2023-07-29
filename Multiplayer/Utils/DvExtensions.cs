using System;
using Multiplayer.Components;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Utils;

public static class DvExtensions
{
    public static ushort GetNetId(this TrainCar car)
    {
        TrainComponentLookup.Instance.NetworkedTrainFromTrain(car, out NetworkedTrainCar networkedTrainCar);
        ushort netId = networkedTrainCar.NetId;
        if (netId == 0)
            throw new InvalidOperationException($"NetId for {car.carLivery.id} ({car.ID}) isn't initialized!");
        return netId;
    }

    public static void SetNetId(this TrainCar car, ushort netId)
    {
        if (netId == 0)
            throw new ArgumentException("NetId cannot be 0");
        TrainComponentLookup.Instance.NetworkedTrainFromTrain(car, out NetworkedTrainCar networkedTrainCar);
        networkedTrainCar.NetId = netId;
    }
}
