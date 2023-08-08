using HarmonyLib;
using I2.Loc;

namespace Multiplayer.Patches.MainMenu;

[HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
public static class LocalizationManager_GetTranslation_Patch
{
    private static bool Prefix(ref string __result, string Term)
    {
        if (!Term.StartsWith(Locale.PREFIX))
            return true;
        __result = Locale.Get(Term);
        return false;
    }
}
