using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(Junction), nameof(Junction.Switch))]
public static class Junction_Switched_Patch
{
    public static bool DontSend;

    private static void Postfix(Junction __instance)
    {
        if (DontSend || !NetworkLifecycle.Instance.IsClientRunning)
            return;
        Junction[] orderedJunctions = WorldData.Instance.OrderedJunctions;
        for (int i = 0; i < orderedJunctions.Length; i++)
        {
            if (orderedJunctions[i] != __instance)
                continue;
            NetworkLifecycle.Instance.Client.SendJunctionSwitched((ushort)i, (byte)__instance.selectedBranch);
            break;
        }
    }
}
