using DV.Tutorial.QT;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(nameof(QuickTutorialInitiator), nameof(QuickTutorialInitiator.IsPlayerOnLocoThatSupportsQuickTutorial))]
public static class QuickTutorialInitiator_IsPlayerOnLocoThatSupportsQuickTutorial_Patch
{
    private static bool Prefix()
    {
        return NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1;
    }
}
