using System;
using System.IO;
using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Editor;
using UnityChan;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer;

public static class Multiplayer
{
    private static UnityModManager.ModEntry ModEntry;
    public static Settings Settings;

    public static AssetIndex AssetIndex { get; private set; }

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

            if (!LoadAssets())
                return false;

            if (typeof(AutoBlink).IsClass)
            {
                // Ensure the UnityChan assembly gets loaded.
            }

            Log("Creating NetworkManager...");
            NetworkLifecycle.CreateLifecycle();
        }
        catch (Exception ex)
        {
            LogException("Failed to load:", ex);
            harmony?.UnpatchAll();
            return false;
        }

        return true;
    }

    private static bool LoadAssets()
    {
        Log("Loading AssetBundle...");
        string assetBundlePath = Path.Combine(ModEntry.Path, "multiplayer.assetbundle");
        if (!File.Exists(assetBundlePath))
        {
            LogError($"AssetBundle not found at '{assetBundlePath}'!");
            return false;
        }

        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
        AssetIndex[] indices = assetBundle.LoadAllAssets<AssetIndex>();
        if (indices.Length != 1)
        {
            LogError("Expected exactly one AssetIndex in the AssetBundle!");
            return false;
        }

        AssetIndex = indices[0];
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

    public static void LogException(object msg, Exception e)
    {
        ModEntry.Logger.LogException($"{msg}", e);
    }

    #endregion
}
