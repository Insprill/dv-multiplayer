using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(BedSleepingController))]
public static class BedSleepingControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BedSleepingController.TurnOffAndEngageHandbrakeOnAllLocos))]
    private static bool TurnOffAndEngageHandbrakeOnAllLocos()
    {
        return CanRun();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BedSleepingController.EngageHandbrakesOnCurrentTrain))]
    private static bool EngageHandbrakesOnCurrentTrain()
    {
        return CanRun();
    }

    private static bool CanRun()
    {
        return NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1;
    }
}
