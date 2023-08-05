using DV.ServicePenalty;
using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Components.SaveGame;

namespace Multiplayer.Patches.SaveGame;

[HarmonyPatch(typeof(CareerManagerDebtController))]
public static class CareerManagerDebtControllerPatch
{
    public static bool HasDebt;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CareerManagerDebtController.Awake))]
    private static void RegisterDebt_Postfix(CareerManagerDebtController __instance)
    {
        __instance.gameObject.AddComponent<NetworkedCareerManagerDebtController>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CareerManagerDebtController.RegisterDebt))]
    private static bool RegisterDebt_Prefix()
    {
        return NetworkLifecycle.Instance.IsHost();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CareerManagerDebtController.UnregisterDebt))]
    private static bool UnregisterDebt_Prefix()
    {
        return NetworkLifecycle.Instance.IsHost();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CareerManagerDebtController.NumberOfNonZeroPricedDebts), MethodType.Getter)]
    private static bool NumberOfNonZeroPricedDebts_Prefix(ref int __result)
    {
        if (NetworkLifecycle.Instance.IsHost())
            return true;
        __result = HasDebt ? 1 : 0;
        return false;
    }
}
