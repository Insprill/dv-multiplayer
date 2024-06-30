using DV.Localization;
using DV.UI;
using DV.UIFramework;
using HarmonyLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Utils;
using System.Reflection;
using TMPro;
using UnityEngine;



namespace Multiplayer.Patches.MainMenu
{
    [HarmonyPatch(typeof(RightPaneController), "OnEnable")]
    public static class RightPaneController_OnEnable_Patch
    {
        public static int hostMenuIndex;
        public static UIMenuController uIMenuController;
        public static HostGamePane hgpInstance;
        private static void Prefix(RightPaneController __instance)
        {
            uIMenuController = __instance.menuController;
            // Check if the multiplayer pane already exists
            if (__instance.HasChildWithName("PaneRight Multiplayer"))
                return;

            // Find the base pane for Load/Save
            GameObject basePane = __instance.FindChildByName("PaneRight Load/Save");
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

            // Add the multiplayer pane to the menu controller
            __instance.menuController.controlledMenus.Add(multiplayerPane.GetComponent<UIMenu>());
            MainMenuController_Awake_Patch.multiplayerButton.GetComponent<UIMenuRequester>().requestedMenuIndex = __instance.menuController.controlledMenus.Count - 1;

            // Clean up unnecessary components and child objects
            GameObject.Destroy(multiplayerPane.GetComponent<SaveLoadController>());
            GameObject.Destroy(multiplayerPane.GetComponent<PlatformSpecificElements>());
            GameObject.Destroy(multiplayerPane.FindChildByName("ButtonIcon OpenFolder"));
            GameObject.Destroy(multiplayerPane.FindChildByName("ButtonIcon Rename"));
            GameObject.Destroy(multiplayerPane.FindChildByName("ButtonTextIcon Load"));
            GameObject.Destroy(multiplayerPane.FindChildByName("Text Content"));

            // Update UI elements
            GameObject titleObj = multiplayerPane.FindChildByName("Title");
            titleObj.GetComponentInChildren<Localize>().key = Locale.SERVER_BROWSER__TITLE_KEY;
            GameObject.Destroy(titleObj.GetComponentInChildren<I2.Loc.Localize>());

            GameObject content = multiplayerPane.FindChildByName("text main");
            //content.GetComponentInChildren<TextMeshProUGUI>().text = "Server browser not yet implemented.";

            GameObject serverWindow = multiplayerPane.FindChildByName("Save Description");
            serverWindow.GetComponentInChildren<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
            serverWindow.GetComponentInChildren<TextMeshProUGUI>().text = "Server browser not <i>fully</i> implemented.<br><br>Dummy servers are shown for demonstration purposes only.<br><br>Press refresh to attempt loading real servers.";

            // Update buttons on the multiplayer pane
            multiplayerPane.UpdateButton("ButtonTextIcon Overwrite", "ButtonTextIcon Manual", Locale.SERVER_BROWSER__MANUAL_CONNECT_KEY, null, Multiplayer.AssetIndex.multiplayerIcon);
            //multiplayerPane.UpdateButton("ButtonTextIcon Load", "ButtonTextIcon Host", Locale.SERVER_BROWSER__HOST_KEY, null, Multiplayer.AssetIndex.lockIcon);
            multiplayerPane.UpdateButton("ButtonTextIcon Save", "ButtonTextIcon Join", Locale.SERVER_BROWSER__JOIN_KEY, null, Multiplayer.AssetIndex.connectIcon);
            GameObject go = multiplayerPane.UpdateButton("ButtonIcon Delete", "ButtonIcon Refresh", Locale.SERVER_BROWSER__REFRESH_KEY, null, Multiplayer.AssetIndex.refreshIcon);


            // Add the MultiplayerPane component
            multiplayerPane.AddComponent<ServerBrowserPane>();

            // Create and initialize MainMenuThingsAndStuff
            MainMenuThingsAndStuff.Create(manager =>
            {
                PopupManager popupManager = null;
                __instance.FindPopupManager(ref popupManager);
                manager.popupManager = popupManager;
                manager.renamePopupPrefab = __instance.continueLoadNewController.career.renamePopupPrefab;
                manager.okPopupPrefab = __instance.continueLoadNewController.career.okPopupPrefab;
                manager.uiMenuController = __instance.menuController;
            });

            // Activate the multiplayer button
            MainMenuController_Awake_Patch.multiplayerButton.SetActive(true);
            Multiplayer.LogError("At end!");








            // Check if the host pane already exists
            if (__instance.HasChildWithName("PaneRight Host"))
                return;

            if (basePane == null)
            {
                Multiplayer.LogError("Failed to find Load/Save pane!");
                return;
            }

            // Create a new host pane based on the base pane
            basePane.SetActive(false);
            GameObject hostPane = GameObject.Instantiate(basePane, basePane.transform.parent);
            basePane.SetActive(true);
            hostPane.name = "PaneRight Host";

            GameObject.Destroy(hostPane.GetComponent<SaveLoadController>());
            GameObject.Destroy(hostPane.GetComponent<PlatformSpecificElements>());
            hgpInstance = hostPane.GetOrAddComponent<HostGamePane>();

            // Add the host pane to the menu controller
            __instance.menuController.controlledMenus.Add(hostPane.GetComponent<UIMenu>());
            hostMenuIndex = __instance.menuController.controlledMenus.Count - 1;
            //MainMenuController_Awake_Patch.multiplayerButton.GetComponent<UIMenuRequester>().requestedMenuIndex = __instance.menuController.controlledMenus.Count - 1;
        }
    }
}
