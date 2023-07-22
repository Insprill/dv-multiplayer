using System;
using Multiplayer.Components;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Utils;

public static class DvExtensions
{
    public static ushort GetNetId(this TrainCar car)
    {
        if (!TrainComponentLookup.Instance.NetworkedTrainFromTrain(car, out NetworkedTrainCar networkedTrainCar))
        {
            TrainComponentLookup.Instance.RegisterTrainCar(car);
            return car.GetComponent<NetworkedTrainCar>().NetId;
        }

        ushort netId = networkedTrainCar.NetId;
        if (netId == 0)
            throw new InvalidOperationException($"NetId for {car.carLivery.id} ({car.ID}) isn't initialized!");
        return netId;
    }

    public static void SetNetId(this TrainCar car, ushort netId)
    {
        if (netId == 0)
            throw new ArgumentException("NetId cannot be 0");

        if (!TrainComponentLookup.Instance.NetworkedTrainFromTrain(car, out NetworkedTrainCar networkedTrainCar))
        {
            TrainComponentLookup.Instance.RegisterTrainCar(car);
            car.GetComponent<NetworkedTrainCar>().NetId = netId;
            return;
        }

        networkedTrainCar.NetId = netId;
    }
}
