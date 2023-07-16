using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(DV.UI.MainMenu), nameof(DV.UI.MainMenu.GoBackToMainMenu))]
public class MainMenu_GoBackToMainMenu_Patch
{
    private static void Prefix()
    {
        NetworkManager.Instance.Stop();
    }
}
