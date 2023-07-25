using System;
using System.Reflection;
using HarmonyLib;
using LocoSim.Implementations;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

public static class SimComponent_Tick_Patch
{
    public static void Patch(Harmony harmony)
    {
        HarmonyMethod prefixMethod = new(AccessTools.DeclaredMethod(typeof(SimComponent_Tick_Patch), nameof(Prefix)));
        foreach (Type type in Assembly.GetAssembly(typeof(SimComponent)).GetTypes())
        {
            if (!type.IsSubclassOf(typeof(SimComponent)))
                continue;
            MethodInfo tickMethod = type.GetMethod(nameof(SimComponent.Tick), AccessTools.all);
            harmony.Patch(tickMethod, prefixMethod);
        }
    }

    private static bool Prefix()
    {
        return NetworkLifecycle.Instance.IsHost();
    }
}
