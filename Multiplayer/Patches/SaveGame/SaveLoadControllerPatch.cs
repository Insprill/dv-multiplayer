using DV.UI;
using DV.UIFramework;
using HarmonyLib;
using System;

namespace Multiplayer.Patches.SaveGame;

[HarmonyPatch(typeof(SaveLoadController))]
public static class SaveLoadControllerPatch
{
    [HarmonyPatch(typeof(SaveLoadController), nameof(SaveLoadController.RefreshInterface), new Type[] { })]
    private static void Postfix(SaveLoadController __instance)
    {
        if (__instance.inMainMenu)
            return;

        // Currently even if the host is playing alone, loading a save game will cause the game to softlock
        // TODO: fix the softlock issue
        if (__instance?.loadButton != null)
        {
            __instance.loadButton.GetComponent<ButtonDV>().ToggleInteractable(false);
        }
    }
}
