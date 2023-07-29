using DV.UIFramework;
using HarmonyLib;
using Multiplayer.Components.MainMenu;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(WorldStreamingInit), nameof(WorldStreamingInit.Awake))]
public static class WorldStreamingInit_Awake_Patch
{
    private static void Postfix()
    {
        if (NetworkLifecycle.Instance.IsClientRunning)
            return;

        if (NetworkLifecycle.Instance.StartServer(Multiplayer.Settings.Port))
            return;

        NetworkLifecycle.Instance.QueueMainMenuEvent(() =>
        {
            Popup popup = MainMenuThingsAndStuff.Instance.ShowOkPopup();
            if (popup == null)
                return;
            popup.labelTMPro.text = "Failed to start server! Ensure that the port is not in use and try again.";
        });

        DV.UI.MainMenu.GoBackToMainMenu();
    }
}
