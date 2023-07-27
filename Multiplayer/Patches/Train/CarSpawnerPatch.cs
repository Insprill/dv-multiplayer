using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.SpawnCar))]
public static class CarSpawner_SpawnCar_Patch
{
    private static void Postfix(TrainCar __result, RailTrack track, Vector3 position, Vector3 forward, bool playerSpawnedCar)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket || !NetworkLifecycle.Instance.IsHost() || UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Server.SendSpawnTrainCar(__result.carLivery, __result.GetNetId(), track, position, forward, playerSpawnedCar);
    }
}
