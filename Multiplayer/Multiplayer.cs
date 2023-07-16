using System;
using System.IO;
using HarmonyLib;
using Multiplayer.Components.Networking;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer;

public static class Multiplayer
{
    private static UnityModManager.ModEntry ModEntry;
    public static Settings Settings;

    public static AssetBundle AssetBundle { get; private set; }

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

            Log("Loading AssetBundle...");
            AssetBundle = AssetBundle.LoadFromFile(Path.Combine(ModEntry.Path, "multiplayer.assetbundle"));

            Log("Creating NetworkManager...");
            NetworkManager.CreateNetworkManager();
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
