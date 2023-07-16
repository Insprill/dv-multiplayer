using System;
using HarmonyLib;
using UnityModManagerNet;

namespace Multiplayer;

public static class Multiplayer
{
    private static UnityModManager.ModEntry ModEntry;
    private static Settings Settings;

    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        ModEntry = modEntry;
        Settings = Settings.Load<Settings>(modEntry);
        ModEntry.OnGUI = Settings.Draw;
        ModEntry.OnSaveGUI = Settings.Save;

        Harmony harmony = null;

        try
        {
            Log("Patching...");
            harmony = new Harmony(ModEntry.Info.Id);
            harmony.PatchAll();
            Log("Successfully patched");
        }
        catch (Exception ex)
        {
            LogException("Failed to load:", ex);
            harmony?.UnpatchAll();
            return false;
        }

        return true;
    }

    #region Logging

    public static void LogDebug(Func<object> resolver)
    {
        if (Settings.VerboseLogging)
            ModEntry.Logger.Log($"[Debug] {resolver.Invoke()}");
    }

    public static void Log(object msg)
    {
        ModEntry.Logger.Log($"[Info] {msg}");
    }

    public static void LogWarning(object msg)
    {
        ModEntry.Logger.Warning($"{msg}");
    }

    public static void LogError(object msg)
    {
        ModEntry.Logger.Error($"{msg}");
    }

    public static void LogCritical(object msg)
    {
        ModEntry.Logger.Critical($"{msg}");
    }

    public static void LogException(object msg, Exception e)
    {
        ModEntry.Logger.LogException($"{msg}", e);
    }

    #endregion
}
