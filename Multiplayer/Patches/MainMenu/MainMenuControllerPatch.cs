using DV.Localization;
using DV.UI;
using HarmonyLib;
using Multiplayer.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Patches.MainMenu;

/// <summary>
/// Harmony patch MainMenuController to add a Multiplayer button.
/// </summary>
[HarmonyPatch(typeof(MainMenuController))]
public static class MainMenuControllerPatch
{
    public static AMainMenuProvider MenuProvider => MainMenuControllerInstance.provider;
    public static GameObject MultiplayerButton { get; private set; }
    public static MainMenuController MainMenuControllerInstance { get; private set; }

    /// <summary>
    /// Prefix method to run before MainMenuController's Awake method.
    /// </summary>
    /// <param name="__instance">The instance of MainMenuController.</param>
    [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.Awake))]
    [HarmonyPrefix]
    private static void Awake(MainMenuController __instance)
    {
        MainMenuControllerInstance = __instance;

        // Find the Sessions button to base the Multiplayer button on
        GameObject sessionsButton = __instance.FindChildByName("ButtonSelectable Sessions");
        if (sessionsButton == null)
        {
            Multiplayer.LogError("Failed to find Sessions button!");
            return;
        }

        // Deactivate the sessions button temporarily to duplicate it
        sessionsButton.SetActive(false);
        MultiplayerButton = Object.Instantiate(sessionsButton, sessionsButton.transform.parent);
        sessionsButton.SetActive(true);

        // Configure the new Multiplayer button
        MultiplayerButton.transform.SetSiblingIndex(sessionsButton.transform.GetSiblingIndex() + 1);
        MultiplayerButton.name = "ButtonSelectable Multiplayer";

        // Set the localization key for the new button
        Localize localize = MultiplayerButton.GetComponentInChildren<Localize>();
        localize.key = Locale.MAIN_MENU__JOIN_SERVER_KEY;

        // Remove existing localization components to reset them
        Object.Destroy(MultiplayerButton.GetComponentInChildren<I2.Loc.Localize>());
        MultiplayerButton.ResetTooltip();

        // Set the icon for the new Multiplayer button
        SetButtonIcon(MultiplayerButton);
    }

    /// <summary>
    /// Sets the icon for the Multiplayer button.
    /// </summary>
    /// <param name="button">The button to set the icon for.</param>
    private static void SetButtonIcon(GameObject button)
    {
        GameObject icon = button.FindChildByName("icon");
        if (icon == null)
        {
            Multiplayer.LogError("Failed to find icon on Sessions button, destroying the Multiplayer button!");
            Object.Destroy(MultiplayerButton);
            return;
        }

        icon.GetComponent<Image>().sprite = Multiplayer.AssetIndex.multiplayerIcon;
    }
}
