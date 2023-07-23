using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(Bogie), nameof(Bogie.FixedUpdate))]
public static class Bogie_FixedUpdate_Patch
{
    private static void Postfix(Bogie __instance)
    {
        if (__instance.carSpawner.PoolSetupInProgress || __instance.HasDerailed || __instance.rb.IsSleeping() || __instance.rb.isKinematic)
            return;
        NetworkLifecycle.Instance.Server?.SendPhysicsUpdate(__instance.Car);
    }
}
