using HarmonyLib;

namespace Multiplayer.Patches.Init;

[HarmonyPatch(typeof(Bootstrap), "SwitchToNextScene")]
public static class BootstrapPatch
{
    private static void Prefix()
    {
        Multiplayer.OnBootstrapped();
    }
}
