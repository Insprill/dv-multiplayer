using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.SaveAllowed))]
public class SaveGameManager_SaveAllowed_Patch
{
    private static bool Prefix(ref bool __result)
    {
        if (!NetworkLifecycle.Instance.IsClientRunning || NetworkLifecycle.Instance.IsHost())
            return true;
        __result = false;
        return false;
    }
}
