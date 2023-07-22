using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TurntableController), nameof(TurntableController.LoadData))]
public static class TurntableController_LoadData_Patch
{
    private static bool Prefix()
    {
        return !NetworkLifecycle.Instance.IsClientRunning || NetworkLifecycle.Instance.IsHost();
    }
}
