using DV.ServicePenalty.UI;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.SaveGame;

[HarmonyPatch(typeof(CareerManagerLicensePayingScreen), nameof(CareerManagerLicensePayingScreen.HandleInputAction))]
public static class CareerManagerLicensePayingScreenPatch
{
    private static bool Prefix(CareerManagerLicensePayingScreen __instance, InputAction input)
    {
        if (input != InputAction.Confirm || NetworkLifecycle.Instance.IsHost())
            return true;
        if (!__instance.cashReg.Buy())
            return false;

        if (__instance.IsJobLicense)
            NetworkLifecycle.Instance.Client.SendLicensePurchaseRequest(__instance.jobLicenseToBuy.id, __instance.IsJobLicense);
        else if (__instance.IsGeneralLicense)
            NetworkLifecycle.Instance.Client.SendLicensePurchaseRequest(__instance.generalLicenseToBuy.id, __instance.IsJobLicense);

        __instance.screenSwitcher.SetActiveDisplay(__instance.licensesScreen);
        return false;
    }
}
