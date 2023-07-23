using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(AppUtil), nameof(AppUtil.RequestSystemOnValueChanged))]
public static class AppUtil_RequestSystemOnValueChanged_Patch
{
    private static bool Prefix(AppUtil __instance, float value)
    {
        return (__instance.IsTimePaused && value < 0.5f) || (NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1);
    }
}
