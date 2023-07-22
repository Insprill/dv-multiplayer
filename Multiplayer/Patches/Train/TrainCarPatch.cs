using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Awake))]
public class TrainCar_Awake_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            return;
        if (__instance.GetComponent<NetworkedTrainCar>() == null)
            __instance.gameObject.AddComponent<NetworkedTrainCar>();
    }
}
