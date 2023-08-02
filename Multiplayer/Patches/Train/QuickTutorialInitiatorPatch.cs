using DV.Tutorial.QT;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(nameof(QuickTutorialInitiator), nameof(QuickTutorialInitiator.WasTutorialAlreadyPlayed))]
public static class QuickTutorialInitiatorPatch
{
    private static bool Prefix()
    {
        return NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1;
    }
}
