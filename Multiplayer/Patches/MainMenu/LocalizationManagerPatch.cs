using HarmonyLib;
using I2.Loc;

namespace Multiplayer.Patches.MainMenu
{
    [HarmonyPatch(typeof(LocalizationManager))]
    public static class LocalizationManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(LocalizationManager.TryGetTranslation))]
        private static bool TryGetTranslation_Prefix(ref bool __result, string Term, out string Translation)
        {
            Translation = string.Empty;

            // Check if the term starts with the specified locale prefix
            if (!Term.StartsWith(Locale.PREFIX))
                return true;

            // Attempt to get the translation for the term
            Translation = Locale.Get(Term);

            // If the translation is missing, set the result to true and skip the original method
            __result = Translation == Locale.MISSING_TRANSLATION;
            return false;
        }
    }
}
