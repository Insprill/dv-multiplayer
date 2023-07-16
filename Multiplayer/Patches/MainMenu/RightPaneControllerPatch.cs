using DV.UI;
using DV.UIFramework;
using HarmonyLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(RightPaneController), "OnEnable")]
public class RightPaneController_OnEnable_Patch
{
    private static void Prefix(RightPaneController __instance)
    {
        if (__instance.FindChildByName("PaneRight Multiplayer"))
            return;
        GameObject launcher = __instance.FindChildByName("PaneRight Launcher");
        if (launcher == null)
        {
            Multiplayer.LogCritical("Failed to find Launcher pane!");
            return;
        }

        launcher.SetActive(false);
        GameObject multiplayerPane = Object.Instantiate(launcher, launcher.transform.parent);
        launcher.SetActive(true);

        multiplayerPane.name = "PaneRight Multiplayer";
        __instance.menuController.controlledMenus.Add(multiplayerPane.GetComponent<UIMenu>());
        MainMenuController_Awake_Patch.MultiplayerButton.GetComponent<UIMenuRequester>().requestedMenuIndex = __instance.menuController.controlledMenus.Count - 1;
        MainMenuController_Awake_Patch.MultiplayerButton.SetActive(true);

        Object.Destroy(multiplayerPane.GetComponent<LauncherController>());
        Object.Destroy(multiplayerPane.FindChildByName("Thumb Background"));
        Object.Destroy(multiplayerPane.FindChildByName("Thumbnail"));
        Object.Destroy(multiplayerPane.FindChildByName("Savegame Details Background"));
        Object.Destroy(multiplayerPane.FindChildByName("ButtonTextIcon Run"));

        MultiplayerPane multiplayer = multiplayerPane.AddComponent<MultiplayerPane>();
        multiplayer.renamePopupPrefab = __instance.continueLoadNewController.career.renamePopupPrefab;
        multiplayer.okPopupPrefab = __instance.continueLoadNewController.career.okPopupPrefab;
        multiplayer.uiMenuController = __instance.menuController;

        multiplayerPane.SetActive(true);
    }
}
