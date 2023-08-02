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
        __instance.GetOrAddComponent<NetworkedTrainCar>();
    }
}
