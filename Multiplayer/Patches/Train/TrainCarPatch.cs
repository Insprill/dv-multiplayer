using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Utils;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Awake))]
public class TrainCar_Awake_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        __instance.gameObject.GetOrAddComponent<NetworkedTrainCar>();
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.SetupRigidbody))]
public class TrainCar_SetupRigidbody_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            __instance.gameObject.GetOrAddComponent<TrainSpeedQueue>();
    }
}
