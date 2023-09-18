using DV.Logic.Job;
using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.Jobs;

[HarmonyPatch(typeof(Station), nameof(Station.AddJobToStation))]
public static class Station_AddJobToStation_Patch
{
    private static void Postfix(Station __instance, Job job)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            return;

        if (!StationComponentLookup.Instance.NetworkedStationFromStation(__instance, out var networkedStation))
            return;

        networkedStation.AddJob(job);
    }
}
