using HarmonyLib;
using Multiplayer.Components.Networking;
using UnityEngine;

namespace Multiplayer.Patches.Player;

[HarmonyPatch(typeof(CustomFirstPersonController), "CharacterMovement")]
public static class CustomFirstPersonController_CharacterMovement_Patch
{
    private static Vector3 lastPosition;
    private static float lastRotationY;

    private static void Postfix(CustomFirstPersonController __instance)
    {
        if (NetworkLifecycle.Instance.Client is not { } client)
            return;
        Transform t = __instance.transform;
        Vector3 position = t.position;
        float rotationY = t.eulerAngles.y;
        if (lastPosition == position && Mathf.Approximately(lastRotationY, rotationY))
            return;
        lastPosition = position;
        lastRotationY = rotationY;
        client.SendPlayerPosition(position, rotationY);
    }
}
