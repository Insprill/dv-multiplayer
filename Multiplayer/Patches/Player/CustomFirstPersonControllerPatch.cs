using HarmonyLib;
using Multiplayer.Components.Networking;
using UnityEngine;

namespace Multiplayer.Patches.Player;

[HarmonyPatch(typeof(CustomFirstPersonController), "CharacterMovement")]
public static class CustomFirstPersonController_CharacterMovement_Patch
{
    private static void Postfix(CustomFirstPersonController __instance)
    {
        if (NetworkLifecycle.Instance.Client is not { } client)
            return;
        Transform t = __instance.transform;
        client.SendPlayerPosition(t.position, t.eulerAngles.y);
    }
}
