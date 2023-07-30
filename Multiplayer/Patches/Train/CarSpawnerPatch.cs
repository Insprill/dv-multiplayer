using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.FireCarAboutToBeDeleted))]
public static class CarSpawner_FireCarAboutToBeDeleted_Patch
{
    private static void Postfix(TrainCar car)
    {
        if (UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Server?.SendDestroyTrainCar(car);
        TrainComponentLookup.Instance.UnregisterTrainCar(car);
    }
}
