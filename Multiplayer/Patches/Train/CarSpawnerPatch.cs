using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.SpawnCar))]
public static class CarSpawner_SpawnCar_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(CarSpawner.SpawnCar))]
    private static void SpawnCar_Postfix(TrainCar __result, RailTrack track, Vector3 position, Vector3 forward, bool playerSpawnedCar)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            return;
        NetworkLifecycle.Instance.Server.SendSpawnTrainCar(__result.carLivery, __result.GetNetId(), track, position, forward, playerSpawnedCar);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CarSpawner.FireCarAboutToBeDeleted))]
    private static void FireCarAboutToBeDeleted_Postfix(TrainCar car)
    {
        if (UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Server?.SendDestroyTrainCar(car);
        TrainComponentLookup.Instance.UnregisterTrainCar(car);
    }
}
