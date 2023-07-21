using System.Collections.Generic;
using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(Junction), nameof(Junction.Switch))]
public static class Junction_Switched_Patch
{
    public static bool DontSend;
    private static readonly Dictionary<Junction, ushort> junctionToIndex = new();

    private static void Postfix(Junction __instance)
    {
        if (DontSend || !NetworkLifecycle.Instance.IsClientRunning)
            return;

        if (!junctionToIndex.TryGetValue(__instance, out ushort index))
            index = CacheJunction(__instance);

        NetworkLifecycle.Instance.Client.SendJunctionSwitched(index, (byte)__instance.selectedBranch);
    }

    private static ushort CacheJunction(Junction __instance)
    {
        ushort index = 0;
        Junction[] orderedJunctions = WorldData.Instance.OrderedJunctions;

        for (int i = 0; i < orderedJunctions.Length; i++)
        {
            if (orderedJunctions[i] != __instance)
                continue;
            index = (ushort)i;
            junctionToIndex.Add(__instance, index);
            break;
        }

        return index;
    }
}
