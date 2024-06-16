using System.Linq;
using System;
using DV.Common;
using DV.Localization;
using DV.Scenarios.Common;
using DV.UI;
using DV.UIFramework;
using HarmonyLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;

namespace Multiplayer.Patches.MainMenu
{
    [HarmonyPatch(typeof(RightPaneController), "OnEnable")]
    public static class RightPaneController_OnEnable_Patch
    {
        private static void Prefix(RightPaneController __instance)
        {
            // Check if the multiplayer pane already exists
            if (__instance.HasChildWithName("PaneRight Multiplayer"))
                return;

            // Find the base pane for Load/Save
            GameObject basePane = __instance.FindChildByName("PaneRight Load/Save");
            //GameObject basePane = __instance.FindChildByName("PaneRight Launcher");
            if (basePane == null)
            {
                Multiplayer.LogError("Failed to find Launcher pane!");
                return;
            }

            // Create a new multiplayer pane based on the base pane
            basePane.SetActive(false);
            GameObject multiplayerPane = GameObject.Instantiate(basePane, basePane.transform.parent);
            basePane.SetActive(true);

            multiplayerPane.name = "PaneRight Multiplayer";

            //multiplayerPane.AddComponent<MultiplayerPane>();

            __instance.menuController.controlledMenus.Add(multiplayerPane.GetComponent<UIMenu>());
            MainMenuController_Awake_Patch.multiplayerButton.GetComponent<UIMenuRequester>().requestedMenuIndex = __instance.menuController.controlledMenus.Count - 1;
            Multiplayer.LogError("before Past Destroyed stuff!");
            // Clean up unnecessary components and child objects
            GameObject.Destroy(multiplayerPane.GetComponent<SaveLoadController>());
            GameObject.Destroy(multiplayerPane.GetComponent<PlatformSpecificElements>());
            GameObject.Destroy(multiplayerPane.FindChildByName("ButtonIcon OpenFolder"));
            GameObject.Destroy(multiplayerPane.FindChildByName("ButtonIcon Rename"));
            GameObject.Destroy(multiplayerPane.FindChildByName("Text Content"));
            Multiplayer.LogError("Past Destroyed stuff!");

            // Update UI elements
            GameObject titleObj = multiplayerPane.FindChildByName("Title");
            titleObj.GetComponentInChildren<Localize>().key = Locale.SERVER_BROWSER__TITLE_KEY;
            GameObject.Destroy(titleObj.GetComponentInChildren<I2.Loc.Localize>());

            GameObject content = multiplayerPane.FindChildByName("text main");
            content.GetComponentInChildren<TextMeshProUGUI>().text = "Server browser not yet implemented.";

            GameObject serverWindow = multiplayerPane.FindChildByName("Save Description");
            serverWindow.GetComponentInChildren<TextMeshProUGUI>().text = "Server information not yet implemented.";

            UpdateButton(multiplayerPane, "ButtonTextIcon Overwrite", "ButtonTextIcon Manual", Locale.SERVER_BROWSER__MANUAL_CONNECT_KEY, null, Multiplayer.AssetIndex.multiplayerIcon);
            UpdateButton(multiplayerPane, "ButtonTextIcon Load", "ButtonTextIcon Host", Locale.SERVER_BROWSER__HOST_KEY, null, Multiplayer.AssetIndex.multiplayerIcon);
            UpdateButton(multiplayerPane, "ButtonTextIcon Save", "ButtonTextIcon Join", Locale.SERVER_BROWSER__JOIN_KEY, null, null);
            UpdateButton(multiplayerPane, "ButtonIcon Delete", "ButtonTextIcon Refresh", Locale.SERVER_BROWSER__REFRESH, null, null);
           
            multiplayerPane.AddComponent<MultiplayerPane>();

            MainMenuThingsAndStuff.Create(manager =>
            {
                PopupManager popupManager = null;
                __instance.FindPopupManager(ref popupManager);
                manager.popupManager = popupManager;
                manager.renamePopupPrefab = __instance.continueLoadNewController.career.renamePopupPrefab;
                manager.okPopupPrefab = __instance.continueLoadNewController.career.okPopupPrefab;
                manager.uiMenuController = __instance.menuController;
            });

            MainMenuController_Awake_Patch.multiplayerButton.SetActive(true);
            Multiplayer.LogError("At end!");
        }

        private static void UpdateButton(GameObject pane, string oldButtonName, string newButtonName, string localeKey, string toolTipKey, Sprite icon)
        {
            GameObject button = pane.FindChildByName(oldButtonName);
            button.name = newButtonName;

            if (button.GetComponentInChildren<Localize>() != null)
            {
                button.GetComponentInChildren<Localize>().key = localeKey;
                GameObject.Destroy(button.GetComponentInChildren<I2.Loc.Localize>());
                ResetTooltip(button);
            }

            if (icon != null)
            {
                SetButtonIcon(button, icon);
            }

            button.GetComponentInChildren<ButtonDV>().ToggleInteractable(true);


        }

        private static void SetButtonIcon(GameObject button, Sprite icon)
        {
            GameObject goIcon = button.FindChildByName("[icon]");
            if (goIcon == null)
            {
                Multiplayer.LogError("Failed to find icon!");
                return;
            }

            goIcon.GetComponent<Image>().sprite = icon;
        }

        private static void ResetTooltip(GameObject button)
        {
            UIElementTooltip tooltip = button.GetComponent<UIElementTooltip>();
            tooltip.disabledKey = null;
            tooltip.enabledKey = null;
        }
    }
}
