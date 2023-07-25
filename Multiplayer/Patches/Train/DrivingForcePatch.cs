using DV.Simulation.Cars;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(DrivingForce), nameof(DrivingForce.Init))]
public static class DrivingForce_Init_Patch
{
    private static bool Prefix(DrivingForce __instance)
    {
        if (NetworkLifecycle.Instance.IsHost())
            return true;
        __instance.enabled = false;
        return false;
    }
}
