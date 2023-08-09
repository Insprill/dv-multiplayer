using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.SaveGame;

[HarmonyPatch(typeof(UnlockablesManager), nameof(UnlockablesManager.UnlockInArray))]
public static class UnlockablesManagerPatch
{
    private static bool Prefix()
    {
        return NetworkLifecycle.Instance.IsHost();
    }
}
