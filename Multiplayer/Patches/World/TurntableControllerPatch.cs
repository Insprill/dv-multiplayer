using HarmonyLib;
using Multiplayer.Components.Networking.World;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TurntableController), nameof(TurntableController.Awake))]
public static class TurntableController_Awake_Patch
{
    private static void Prefix(TurntableController __instance)
    {
        __instance.turntable.gameObject.AddComponent<NetworkedTurntable>();
    }
}
