using DV.Interaction;
using HarmonyLib;
using Multiplayer.Components.Networking.World;
using System.Collections;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(GenericSwitch))]
public class GenericSwitchPatch
{
    [HarmonyPatch(typeof(GenericSwitch), MethodType.Constructor)]
    [HarmonyPostfix]

    public static void GenericSwitch_Constructor(GenericSwitch __instance)
    {
        Multiplayer.LogDebug(() => $"GenericSwitch.Constructor() persistenceKey: {__instance.persistenceKey}");
        CoroutineManager.Instance.StartCoroutine(WaitForGenericSwitch(__instance));
    }

    private static IEnumerator WaitForGenericSwitch(GenericSwitch genericSwitch)
    {

        while (string.IsNullOrEmpty(genericSwitch.persistenceKey))
                yield return new WaitForEndOfFrame();

        Multiplayer.LogDebug(() => $"WaitForGenericSwitch() persistenceKey: {genericSwitch.persistenceKey}");

        genericSwitch.gameObject.AddComponent<NetworkedGenericSwitch>();
    }
}
