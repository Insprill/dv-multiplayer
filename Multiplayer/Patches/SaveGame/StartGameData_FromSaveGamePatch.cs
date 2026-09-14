using HarmonyLib;
using Multiplayer.Utils;

namespace Multiplayer.Patches.SaveGame;

#if DEBUG
[HarmonyPatch(typeof(StartGameData_FromSaveGame))]
public static class StartGameData_FromSaveGamePatch
{
    [HarmonyPatch(nameof(StartGameData_FromSaveGame.GetSaveGameData))]
    [HarmonyPostfix]
    public static void GetSaveGameData()
    {
        if (Multiplayer.Settings.ExportSaveOnLoad)
            ExportSaveData.DumpSaveData(false);
    }
}
#endif
