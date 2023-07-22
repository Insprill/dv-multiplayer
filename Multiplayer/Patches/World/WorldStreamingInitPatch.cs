using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(WorldStreamingInit), nameof(WorldStreamingInit.Awake))]
public static class WorldStreamingInit_Awake_Patch
{
    private static void Postfix()
    {
        if (!NetworkLifecycle.Instance.IsClientRunning)
            NetworkLifecycle.Instance.StartServer(Multiplayer.Settings.Port);
    }
}
