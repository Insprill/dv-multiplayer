using HarmonyLib;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(GraphicsOptions))]
public static class GraphicsOptionsPatch
{
    // When the Pause Menu is opened on the host and the host alt+tabs, the simulation is paused
    // This patch prevents the pause from occurring if more than one player is connected to the host
    [HarmonyPatch(nameof(GraphicsOptions.UpdateRunInBackground))]
    [HarmonyPostfix]
    private static void UpdateRunInBackground_Postfix(GraphicsOptions __instance)
    {
        if (LoadingScreenManager.IsLoading)
            return;

        if (!DvExtensions.AllowPause())
            Application.runInBackground = true;
    }
}
