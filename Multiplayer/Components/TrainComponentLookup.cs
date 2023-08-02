using System.Collections;
using System.Collections.Generic;
using DV;
using DV.Simulation.Brake;
using DV.ThingTypes;
using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Components;

public class TrainComponentLookup : SingletonBehaviour<TrainComponentLookup>
{
    private readonly Dictionary<string, TrainCarLivery> liveryIdToLivery = new();
    private readonly Dictionary<HoseAndCock, Coupler> hoseToCoupler = new();
    private readonly Dictionary<TrainCar, NetworkedTrainCar> trainToNetworkedTrain = new();
    private readonly Dictionary<ushort, NetworkedTrainCar> netIdToNetworkedTrain = new();
    private readonly Dictionary<ushort, TrainCar> netIdToTrainCar = new();

    public bool LiveryFromId(string liveryId, out TrainCarLivery livery)
    {
        if (liveryIdToLivery.TryGetValue(liveryId, out livery))
            return true;
        TrainCarLivery l = Globals.G.Types.Liveries.Find(l => l.id == liveryId);
        if (l == null)
            return false;
        livery = l;
        liveryIdToLivery[liveryId] = l;
        return true;
    }

    public void RegisterTrainCar(NetworkedTrainCar networkedTrainCar)
    {
        trainToNetworkedTrain[networkedTrainCar.TrainCar] = networkedTrainCar;
        netIdToTrainCar[networkedTrainCar.NetId] = networkedTrainCar.TrainCar;
        netIdToNetworkedTrain[networkedTrainCar.NetId] = networkedTrainCar;
        StartCoroutine(RegisterCouplers(networkedTrainCar));
    }

    private IEnumerator RegisterCouplers(NetworkedTrainCar networkedTrainCar)
    {
        while (networkedTrainCar.TrainCar.couplers == null || networkedTrainCar.TrainCar.couplers.Length == 0)
            yield return WaitFor.EndOfFrame;
        foreach (Coupler coupler in networkedTrainCar.TrainCar.couplers)
            hoseToCoupler[coupler.hoseAndCock] = coupler;
    }

    public void UnregisterTrainCar(TrainCar trainCar)
    {
        NetworkedTrainCar networkedTrainCar = trainToNetworkedTrain[trainCar];
        trainToNetworkedTrain.Remove(trainCar);
        netIdToNetworkedTrain.Remove(networkedTrainCar.NetId);
        netIdToTrainCar.Remove(networkedTrainCar.NetId);
        foreach (Coupler coupler in trainCar.couplers)
            hoseToCoupler.Remove(coupler.hoseAndCock);
    }

    public bool CouplerFromHose(HoseAndCock hose, out Coupler coupler)
    {
        return hoseToCoupler.TryGetValue(hose, out coupler);
    }

    public bool TrainFromNetId(ushort netId, out TrainCar trainCar)
    {
        return netIdToTrainCar.TryGetValue(netId, out trainCar);
    }

    public bool NetworkedTrainFromNetId(ushort netId, out NetworkedTrainCar networkedTrainCar)
    {
        return netIdToNetworkedTrain.TryGetValue(netId, out networkedTrainCar);
    }

    public bool NetworkedTrainFromTrain(TrainCar trainCar, out NetworkedTrainCar networkedTrainCar)
    {
        return trainToNetworkedTrain.TryGetValue(trainCar, out networkedTrainCar);
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(TrainComponentLookup)}]";
    }
}
