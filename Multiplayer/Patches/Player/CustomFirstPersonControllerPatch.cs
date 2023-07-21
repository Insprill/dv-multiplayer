using System.Collections;
using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Networking.Listeners;
using UnityEngine;

namespace Multiplayer.Patches.Player;

[HarmonyPatch(typeof(CustomFirstPersonController))]
public static class MovementSyncPatch
{
    private const byte UPDATE_RATE = 20;
    private const float targetDeltaTime = 1.0f / UPDATE_RATE;

    private static Vector3 lastPosition;
    private static float lastRotationY;
    private static bool sentFinalPosition;

    private static WaitForSeconds frameDelay;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.Awake))]
    private static void CharacterMovement(CustomFirstPersonController __instance)
    {
        frameDelay = new WaitForSeconds(targetDeltaTime);
        CoroutineManager.Instance.StartCoroutine(WaitForClient(__instance));
    }

    private static IEnumerator WaitForClient(CustomFirstPersonController __instance)
    {
        while (!NetworkLifecycle.Instance.IsClientRunning)
            yield return null;
        CoroutineManager.Instance.StartCoroutine(SendPositionCoro(__instance, NetworkLifecycle.Instance.Client));
    }

    private static IEnumerator SendPositionCoro(CustomFirstPersonController __instance, NetworkClient client)
    {
        // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
        while (__instance != null)
        {
            if (client is not { IsRunning: true })
            {
                yield return null;
                continue;
            }

            Transform t = __instance.transform;
            Vector3 position = t.position;
            float rotationY = t.eulerAngles.y;

            bool positionOrRotationChanged = lastPosition != position || !Mathf.Approximately(lastRotationY, rotationY);
            if (positionOrRotationChanged || !sentFinalPosition)
            {
                lastPosition = position;
                lastRotationY = rotationY;
                sentFinalPosition = !positionOrRotationChanged;
                client.SendPlayerPosition(position, lastRotationY, false, sentFinalPosition);
            }

            yield return frameDelay;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.SetJumpParameters))]
    private static void SetJumpParameters(CustomFirstPersonController __instance)
    {
        if (NetworkLifecycle.Instance.Client is not { } client)
            return;
        Transform t = __instance.transform;
        lastPosition = t.position;
        lastRotationY = t.eulerAngles.y;
        client.SendPlayerPosition(lastPosition, lastRotationY, true, true);
    }
}
