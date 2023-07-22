using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(JunctionsSaveManager), nameof(JunctionsSaveManager.Load))]
public static class JunctionsSaveManager_Load_Patch
{
    private static bool Prefix()
    {
        return !NetworkLifecycle.Instance.IsClientRunning || NetworkLifecycle.Instance.IsHost();
    }
}
