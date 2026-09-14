using DV;
using HarmonyLib;
using Multiplayer.Utils;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(AppUtil))]
public static class AppUtilPatch
{
    [HarmonyPatch(nameof(AppUtil.RequestSystemOnValueChanged))]
    [HarmonyPrefix]
    private static bool RequestSystemOnValueChanged(AppUtil __instance, float value)
    {
        return (__instance.IsTimePaused && value < 0.5f) || DvExtensions.AllowPause();
    }

    [HarmonyPatch(nameof(AppUtil.RequestPause))]
    [HarmonyPrefix]
    private static bool RequestPause(AppUtil __instance, object caller, bool paused, int priority)
    {
        return !paused || DvExtensions.AllowPause();
    }
}
