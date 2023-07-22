using DV.ThingTypes;
using HarmonyLib;
using Multiplayer.Components.Networking;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.SpawnCar))]
public static class CarSpawner_SpawnCar_Patch
{
    public static bool DontSend;

    private static void Postfix(GameObject carToSpawn, RailTrack track, Vector3 position, Vector3 forward, bool playerSpawnedCar)
    {
        if (DontSend || !NetworkLifecycle.Instance.IsHost())
            return;
        TrainCarLivery livery = carToSpawn.GetComponent<TrainCar>().carLivery;
        NetworkLifecycle.Instance.Server.SendSpawnTrainCar(livery, track, position, forward, playerSpawnedCar);
    }
}
