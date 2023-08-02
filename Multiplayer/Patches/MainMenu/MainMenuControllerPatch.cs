using DV.Localization;
using DV.UI;
using HarmonyLib;
using Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(MainMenuController), "Awake")]
public static class MainMenuController_Awake_Patch
{
    public static GameObject MultiplayerButton;

    private static void Prefix(MainMenuController __instance)
    {
        GameObject button = __instance.FindChildByName("ButtonSelectable Sessions");
        if (button == null)
        {
            Multiplayer.LogError("Failed to find Sessions button!");
            return;
        }

        MultiplayerButton = Object.Instantiate(button, button.transform.parent);
        MultiplayerButton.transform.SetSiblingIndex(button.transform.GetSiblingIndex() + 1);
        MultiplayerButton.name = "ButtonSelectable Multiplayer";

        Object.Destroy(MultiplayerButton.GetComponentInChildren<Localize>());

        TMP_Text text = MultiplayerButton.GetComponentInChildren<TMP_Text>();
        text.text = "Join Server";

        GameObject icon = MultiplayerButton.FindChildByName("icon");
        if (icon == null)
        {
            Multiplayer.LogError("Failed to find icon on Sessions button!");
            Object.Destroy(MultiplayerButton);
            return;
        }

        icon.GetComponent<Image>().sprite = Multiplayer.AssetIndex.multiplayerIcon;
    }
}
