using System.Collections.Generic;
using DV.Simulation.Brake;
using DV.Utils;
using JetBrains.Annotations;

namespace Multiplayer.Components;

public class TrainComponentLookup : SingletonBehaviour<TrainComponentLookup>
{
    private readonly Dictionary<string, TrainCar> guidToTrainCar = new();
    private readonly Dictionary<HoseAndCock, Coupler> hoseToCoupler = new();

    public void RegisterHose(HoseAndCock hose, Coupler coupler)
    {
        hoseToCoupler[hose] = coupler;
    }

    public Coupler CouplerFromHose(HoseAndCock hose)
    {
        return hoseToCoupler[hose];
    }

    public void RegisterTrainCarGUID(TrainCar trainCar)
    {
        guidToTrainCar[trainCar.CarGUID] = trainCar;
    }

    public void UnregisterTrainCarGUID(TrainCar trainCar)
    {
        guidToTrainCar.Remove(trainCar.CarGUID);
    }

    public bool TrainCarFromGUID(string guid, out TrainCar trainCar)
    {
        return guidToTrainCar.TryGetValue(guid, out trainCar);
    }

        [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"{nameof(TrainComponentLookup)}";
    }
}
