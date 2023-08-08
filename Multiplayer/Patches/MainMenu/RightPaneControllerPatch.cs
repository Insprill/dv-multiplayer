using DV.Localization;
using DV.UI;
using DV.UIFramework;
using HarmonyLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(RightPaneController), "OnEnable")]
public static class RightPaneController_OnEnable_Patch
{
    private static void Prefix(RightPaneController __instance)
    {
        if (__instance.HasChildWithName("PaneRight Multiplayer"))
            return;
        GameObject basePane = __instance.FindChildByName("PaneRight Settings");

        basePane.SetActive(false);
        GameObject multiplayerPane = Object.Instantiate(basePane, basePane.transform.parent);
        basePane.SetActive(true);

        multiplayerPane.name = "PaneRight Multiplayer";
        __instance.menuController.controlledMenus.Add(multiplayerPane.GetComponent<UIMenu>());
        MainMenuController_Awake_Patch.MultiplayerButton.GetComponent<UIMenuRequester>().requestedMenuIndex = __instance.menuController.controlledMenus.Count - 1;

        Object.Destroy(multiplayerPane.GetComponent<SettingsController>());
        Object.Destroy(multiplayerPane.GetComponent<PlatformSpecificElements>());
        Object.Destroy(multiplayerPane.FindChildByName("Left Buttons"));
        Object.Destroy(multiplayerPane.FindChildByName("Text Content"));

        GameObject rightSubMenus = multiplayerPane.FindChildByName("Right Submenus");
        GameObject languagePane = rightSubMenus.FindChildByName("PaneRight Language");

        RectTransform langRect = languagePane.GetComponent<RectTransform>();
        RectTransform subMenusRect = rightSubMenus.GetComponent<RectTransform>();

        Vector2 sizeDelta = new(1290, 600);
        subMenusRect.sizeDelta = sizeDelta;
        langRect.sizeDelta = sizeDelta;

        foreach (GameObject go in rightSubMenus.GetChildren())
        {
            if (go.name == "PaneRight Language")
                continue;
            Object.Destroy(go);
        }

        GameObject viewport = languagePane.FindChildByName("Viewport");
        foreach (GameObject go in viewport.GetChildren())
            Object.Destroy(go);

        Object.Destroy(languagePane.FindChildByName("Title"));
        Object.Destroy(languagePane.FindChildByName("Help button"));
        Object.Destroy(languagePane.FindChildByName("Text Content"));
        Object.Destroy(languagePane.FindChildByName("ButtonTextIcon"));
        Object.Destroy(languagePane.GetComponent<LanguageSelectorController>());
        Object.Destroy(languagePane.GetComponent<SettingChangeSourceLanguage>());
        Object.Destroy(multiplayerPane.FindChildByName("Selector Preset"));
        Object.Destroy(multiplayerPane.FindChildByName("ButtonTextIcon Discard"));

        GameObject manualConnect = multiplayerPane.FindChildByName("ButtonTextIcon Apply");
        manualConnect.GetComponentInChildren<Localize>().key = Locale.SERVER_BROWSER__MANUAL_CONNECT_KEY;
        Object.Destroy(manualConnect.GetComponentInChildren<I2.Loc.Localize>());

        GameObject titleObj = multiplayerPane.FindChildByName("Title");
        titleObj.GetComponentInChildren<Localize>().key = Locale.SERVER_BROWSER__TITLE_KEY;
        Object.Destroy(titleObj.GetComponentInChildren<I2.Loc.Localize>());

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

        multiplayerPane.SetActive(true);
        MainMenuController_Awake_Patch.MultiplayerButton.SetActive(true);
    }
}
