using System.Collections.Generic;
using DV.Simulation.Brake;
using DV.Utils;

namespace Multiplayer.Components;

public class TrainComponentLookup : SingletonBehaviour<TrainComponentLookup>
{
    private readonly Dictionary<HoseAndCock, Coupler> hoseToCoupler = new();

    public void RegisterHose(HoseAndCock hose, Coupler coupler)
    {
        hoseToCoupler[hose] = coupler;
    }

    public Coupler CouplerFromHose(HoseAndCock hose)
    {
        return hoseToCoupler[hose];
    }
}
