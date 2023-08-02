using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.ActuallyDeletingTrainCar))]
public static class CarSpawner_ActuallyDeletingTrainCar_Patch
{
    private static void Postfix(TrainCar trainCar)
    {
        if (UnloadWatcher.isUnloading)
            return;
        if (!TrainComponentLookup.Instance.NetworkedTrainFromTrain(trainCar, out NetworkedTrainCar _))
            return;
        NetworkLifecycle.Instance.Server?.SendDestroyTrainCar(trainCar);
        TrainComponentLookup.Instance.UnregisterTrainCar(trainCar);
    }
}
