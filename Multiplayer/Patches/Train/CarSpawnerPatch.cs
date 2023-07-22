using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.SpawnCar))]
public static class CarSpawner_SpawnCar_Patch
{
    private static void Postfix(TrainCar __result, RailTrack track, Vector3 position, Vector3 forward, bool playerSpawnedCar)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket || !NetworkLifecycle.Instance.IsHost())
            return;
        NetworkedTrainCar networkedTrainCar = __result.GetComponent<NetworkedTrainCar>();
        NetworkLifecycle.Instance.Server.SendSpawnTrainCar(__result.carLivery, networkedTrainCar.NetId, track, position, forward, playerSpawnedCar);
    }
}
