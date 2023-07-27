using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

// We can't easily patch SetField since it's generic (https://harmony.pardeike.net/articles/patching-edgecases.html#generics).
[HarmonyPatch(typeof(GameParams), nameof(GameParams.OnPropertyChanged))]
public static class GameParams_OnPropertyChanged_Patch
{
    private static void Postfix(GameParams __instance)
    {
        if (!NetworkLifecycle.Instance.IsHost() || __instance != Globals.G.GameParams)
            return;
        NetworkLifecycle.Instance.Server.SendGameParams(__instance);
    }
}
