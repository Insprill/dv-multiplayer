using HarmonyLib;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(DisplayLoadingInfo), nameof(DisplayLoadingInfo.Start))]
public static class DisplayLoadingInfo_Start_Patch
{
    private static void Postfix(DisplayLoadingInfo __instance)
    {
        WorldStreamingInit.LoadingFinished -= __instance.OnLoadingFinished;
    }
}
