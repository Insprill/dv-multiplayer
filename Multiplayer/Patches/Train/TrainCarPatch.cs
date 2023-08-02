using HarmonyLib;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Utils;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TrainCar))]
public class TrainCarPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrainCar.Awake))]
    private static void Awake_Prefix(TrainCar __instance)
    {
        InitNetworkedTrainCar(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TrainCar.AwakeForPooledCar))]
    private static void AwakeForPooledCar_Prefix(TrainCar __instance)
    {
        InitNetworkedTrainCar(__instance);
    }

    private static void InitNetworkedTrainCar(TrainCar __instance)
    {
        if (CarSpawner.Instance.PoolSetupInProgress)
            return;
        if (__instance.gameObject.GetComponent<NetworkedTrainCar>())
            Multiplayer.LogDebug(() => $"{__instance.carLivery.id} already has a NetworkedTrainCar before awake");
        else
            Multiplayer.LogDebug(() => $"Adding NetworkedTrainCar to {__instance.carLivery.id} before awake");
        __instance.GetOrAddComponent<NetworkedTrainCar>();
    }
}
