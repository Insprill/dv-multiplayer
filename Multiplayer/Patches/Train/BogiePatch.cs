using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Multiplayer.Patches.Train;

[HarmonyPatch(typeof(Bogie))]
public static class BogiePatch
{

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Bogie.UpdatePointSetTraveller))]
    private static IEnumerable<CodeInstruction> UpdatePointSetTraveller(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        // Find the Debug.LogError call and remove it along with its argument preparation
        for (int i = 0; i < codes.Count; i++)
        {
            // Look for the Debug.LogError call
            if (codes[i].opcode == OpCodes.Call &&
                codes[i].operand is MethodInfo method &&
                method.DeclaringType == typeof(Debug) &&
                method.Name == nameof(Debug.LogError))
            {
                // Remove the 5 instructions that prepare and call LogError:
                // ldstr, ldarg.1, box, call String.Format, call Debug.LogError
                if (i >= 4)
                {
                    codes.RemoveRange(i - 4, 5);
                    break;
                }
            }
        }

        return codes;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Bogie.SetupPhysics))]
    private static void SetupPhysics(Bogie __instance)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            __instance.gameObject.GetOrAddComponent<NetworkedBogie>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Bogie.SwitchJunctionIfNeeded))]
    private static bool SwitchJunctionIfNeeded()
    {
        return NetworkLifecycle.Instance.IsHost();
    }
}

