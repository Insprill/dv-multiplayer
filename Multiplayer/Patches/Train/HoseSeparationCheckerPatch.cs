using DV.Simulation.Brake;
using HarmonyLib;
using Multiplayer.Components.Networking;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace Multiplayer.Patches.Train;

[HarmonyPatch(typeof(HoseSeparationChecker))]
public class HoseSeparationCheckerPatch
{
    [HarmonyPatch(nameof(HoseSeparationChecker.CheckDistances))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CheckDistances(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        codeMatcher
            .MatchStartForward
            (
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector3), "op_Subtraction", [typeof(Vector3), typeof(Vector3)]))
            ).Repeat(m =>
                m.ThrowIfNotMatch("Failed to find reference to Vector3.op_Subtraction() in HoseSeparationChecker.CheckDistances")
                .Set(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HoseSeparationCheckerPatch), nameof(GetSeparationDistance)))
                .Advance(1)
                .RemoveInstruction() // Remove the call to Vector3.get_sqrMagnitude
            );

        return codeMatcher.Instructions();
    }

    private static float GetSeparationDistance(Vector3 a, Vector3 b)
    {
        // Allow the host to calculate the actual distance
        if (NetworkLifecycle.Instance.IsHost())
            return (a - b).sqrMagnitude;

        // Clients return 0 to ensure the separation check does not trigger during lag events
        return 0;
    }

}
