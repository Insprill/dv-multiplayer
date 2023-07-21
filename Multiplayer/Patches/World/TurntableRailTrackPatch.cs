using System.Collections.Generic;
using HarmonyLib;
using Multiplayer.Components.Networking;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TurntableRailTrack), nameof(TurntableRailTrack.RotateToTargetRotation))]
public static class TurntableRailTrack_RotateToTargetRotation_Patch
{
    public static bool DontSend;
    private static readonly Dictionary<TurntableRailTrack, byte> turntableToIndex = new();

    private static void Prefix(TurntableRailTrack __instance, bool forceConnectionRefresh)
    {
        if (DontSend || !NetworkLifecycle.Instance.IsClientRunning)
            return;

        float difference = __instance.targetYRotation - __instance.currentYRotation;
        if (Mathf.Approximately(Mathf.Abs(difference), 0.0f) && !forceConnectionRefresh)
            return;

        if (!turntableToIndex.TryGetValue(__instance, out byte index))
            index = CacheTurntable(__instance);

        NetworkLifecycle.Instance.Client.SendTurntableRotation(index, __instance.targetYRotation);
    }

    private static byte CacheTurntable(TurntableRailTrack __instance)
    {
        byte index = 0;
        for (int i = 0; i < TurntableController.allControllers.Count; i++)
        {
            if (i > byte.MaxValue)
            {
                Multiplayer.LogWarning($"There's more than {byte.MaxValue} turntables on the map, the index should be changed to a ushort!");
                break;
            }

            TurntableRailTrack turntableRailTrack = TurntableController.allControllers[i].turntable;
            if (turntableRailTrack != __instance)
                continue;

            index = (byte)i;
            turntableToIndex.Add(__instance, index);
        }

        return index;
    }
}
