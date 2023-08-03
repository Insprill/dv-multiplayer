using System;
using Multiplayer.Components;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Components.Networking.World;

namespace Multiplayer.Utils;

public static class DvExtensions
{
    public static ushort GetNetId(this TrainCar car)
    {
        ushort netId = car.Networked().NetId;
        if (netId == 0)
            throw new InvalidOperationException($"NetId for {car.carLivery.id} ({car.ID}) isn't initialized!");
        return netId;
    }

    public static NetworkedTrainCar Networked(this TrainCar car)
    {
        TrainComponentLookup.Instance.NetworkedTrainFromTrain(car, out NetworkedTrainCar networkedTrainCar);
        return networkedTrainCar;
    }

    public static NetworkedRailTrack Networked(this RailTrack railTrack)
    {
        return NetworkedRailTrack.GetFromRailTrack(railTrack);
    }
}
