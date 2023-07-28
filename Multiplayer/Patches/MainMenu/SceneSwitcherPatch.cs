using HarmonyLib;

namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(SceneSwitcher), nameof(SceneSwitcher.LoadScene))]
public static class SceneSwitcherPatch
{
    private static void Postfix()
    {
        // SceneSwitcher#SwitchToScene calls AssetBundle#UnloadAllAssetBundles when returning
        // to the main menu, so we need to load our assets again.
        Multiplayer.LoadAssets();
    }
}
