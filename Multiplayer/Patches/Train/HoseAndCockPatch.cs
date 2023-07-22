using DV.Simulation.Brake;
using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(HoseAndCock), nameof(HoseAndCock.SetCock))]
public static class HoseAndCock_SetCock_Patch
{
    private static void Prefix(HoseAndCock __instance, bool open)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        Coupler coupler = TrainComponentLookup.Instance.CouplerFromHose(__instance);
        NetworkLifecycle.Instance.Client?.SendCockState(coupler, open);
    }
}
