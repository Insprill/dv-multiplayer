using DV.UI;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.Train;

[HarmonyPatch(typeof(FastTravelUIController))]
public static class FastTravelUIControllerPatch
{
    [HarmonyPatch(typeof(FastTravelUIController), nameof(FastTravelUIController.RefreshInterface))]
    private static void Postfix(FastTravelUIController __instance)
    {
        // If the host is playing alone, don't disable the fast travel with loco button
        if (NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1)
            return;

        if (__instance?.fastTravelWithLocoButton != null)
        {
            __instance.fastTravelWithLocoButton.ToggleInteractable(false);
        }
    }
}
