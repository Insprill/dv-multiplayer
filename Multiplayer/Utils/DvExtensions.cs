using System;
using Multiplayer.Components;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Utils;

public static class DvExtensions
{
    public static ushort GetNetId(this TrainCar car)
    {
        ushort netId = car.GetNetworkedCar().NetId;
        if (netId == 0)
            throw new InvalidOperationException($"NetId for {car.carLivery.id} ({car.ID}) isn't initialized!");
        return netId;
    }

    public static NetworkedTrainCar GetNetworkedCar(this TrainCar car)
    {
        TrainComponentLookup.Instance.NetworkedTrainFromTrain(car, out NetworkedTrainCar networkedTrainCar);
        return networkedTrainCar;
    }
}
