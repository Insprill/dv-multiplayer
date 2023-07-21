using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TimeAdvance), nameof(TimeAdvance.AdvanceTime))]
public static class TimeAdvance_AdvanceTime_Patch
{
    public static bool DontSend;

    private static void Prefix(float amountOfTimeToSkipInSeconds)
    {
        if (DontSend || !NetworkLifecycle.Instance.IsClientRunning)
            return;
        NetworkLifecycle.Instance.Client.SendTimeAdvance(amountOfTimeToSkipInSeconds);
    }
}
