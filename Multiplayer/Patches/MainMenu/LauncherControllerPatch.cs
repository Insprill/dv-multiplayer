using DV.Localization;
using DV.UI;
using DV.UIFramework;
using HarmonyLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Utils;
using UnityEngine;
using UnityEngine.UI;


namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(LauncherController), "OnEnable")]
public static class LauncherController_Patch
{
    private const int PADDING = 10;
    
    private static GameObject goHost;

    private static void Postfix(LauncherController __instance)
    {

        Multiplayer.Log("LauncherController_Patch()");

        if (goHost != null)
            return;

        GameObject goRun = __instance.FindChildByName("ButtonTextIcon Run");

        if(goRun != null)
        {
            goRun.SetActive(false);
            goHost = GameObject.Instantiate(goRun);
            goRun.SetActive(true);

            goHost.name = "ButtonTextIcon Host";
            goHost.transform.SetParent(goRun.transform.parent, false);

            RectTransform btnHostRT = goHost.GetComponentInChildren<RectTransform>();

            Vector3 curPos = btnHostRT.localPosition;
            Vector2 curSize = btnHostRT.sizeDelta;

            btnHostRT.localPosition = new Vector3(curPos.x - curSize.x - PADDING, curPos.y,curPos.z);

            __instance.transform.gameObject.UpdateButton("ButtonTextIcon Host", "ButtonTextIcon Host", Locale.SERVER_BROWSER__HOST_KEY, null, Multiplayer.AssetIndex.lockIcon);


            // Set up event listeners
            Button btnHost = goHost.GetComponent<ButtonDV>();
            //UIMenuRequester uim = btnHost.GetOrAddComponent<UIMenuRequester>();
            //uim.targetMenuController = RightPaneController_OnEnable_Patch.uIMenuController;
            //uim.requestedMenuIndex = RightPaneController_OnEnable_Patch.hostMenuIndex;

            btnHost.onClick.AddListener(HostAction);

            goHost.SetActive(true);

            Multiplayer.Log("LauncherController_Patch() complete");
        }
    }

    private static void HostAction()
    {
        // Implement host action logic here
        Debug.Log("Host button clicked.");
        // Add your code to handle hosting a game

        RightPaneController_OnEnable_Patch.uIMenuController.SwitchMenu(RightPaneController_OnEnable_Patch.hostMenuIndex);

    }
}
