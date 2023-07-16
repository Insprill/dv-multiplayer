using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

// PlayerInstantiator Non-VR: CustomFPSControllerNonVR
// PlayerInstantiator VR: VRTK_main

[HarmonyPatch(typeof(WorldStreamingInit), "Awake")]
public static class WorldStreamingInit_Awake_Patch
{
    private static void Prefix()
    {
        WorldStreamingInit.LoadingFinished += OnWorldLoaded;
    }

    private static void OnWorldLoaded()
    {
        // World moving is hard-disabled via the WorldMoverPatch, but we update this anyway so scripts are aware of that.
        WorldMover.Instance.movingEnabled = false;

        NetworkManager.Instance.StartServer(Multiplayer.Settings.Port);
        // todo: start hosting
    }
}
