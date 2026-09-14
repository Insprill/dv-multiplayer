using DV.Customization.Paint;
using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class PaintThemeLookup : SingletonBehaviour<PaintThemeLookup>
{
    private readonly Dictionary<uint, string> hashToThemeName = [];
    private readonly Dictionary<string, uint> themeNameToHash = [];
    private readonly Dictionary<uint, string> hashToBaseThemeName = [];
    private readonly HashSet<string> baseThemeNamesSet = [];

    private bool moddedThemesSearched = false;

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(PaintThemeLookup)}]";
    }

    protected override void Awake()
    {
        base.Awake();

        var themes = Resources.LoadAll<PaintTheme>("");

        foreach (var theme in themes)
        {
            if (theme != null && !string.IsNullOrEmpty(theme.AssetName))
            {
                var id = RegisterTheme(theme);
                if (id != 0)
                    hashToBaseThemeName[id] = theme.AssetName;
            }
        }

        baseThemeNamesSet.UnionWith(hashToBaseThemeName.Values);
    }

    #region Public Methods
    public bool TryGetNetId(PaintTheme theme, out uint netId)
    {
        netId = 0;

        if (theme == null)
            return false;

        if (themeNameToHash.TryGetValue(theme.AssetName, out netId))
            return true;

        netId = GetThemeId(theme.AssetName);

        if (hashToThemeName.ContainsKey(netId))
            return true;

        // Skin Manager might not have been updated for Multiplayer yet, or another mod may have added themes
        // Try to find themes added by mods
        FindModdedThemes();

        if (hashToThemeName.ContainsKey(netId))
            return true;

        netId = 0;
        return false;
    }

    public bool TryGet(uint themeId, out PaintTheme paintTheme)
    {
        paintTheme = null;

        if (!hashToThemeName.TryGetValue(themeId, out string themeName))
        {
            // Skin Manager might not have been updated for Multiplayer yet, or another mod may have added themes
            // Try to find themes added by mods
            FindModdedThemes();

            hashToThemeName.TryGetValue(themeId, out themeName);
        }

        if (themeName == null)
            return false;

        return PaintTheme.TryLoad(themeName, out paintTheme);
    }

    public uint RegisterTheme(PaintTheme theme)
    {
        if (theme == null || string.IsNullOrEmpty(theme.AssetName))
            return 0;

        var hash = GetThemeId(theme.AssetName);

        if (hash == 0 || hash == uint.MaxValue)
            return 0;

        if (hashToThemeName.TryGetValue(hash, out var existingTheme))
        {
            // Check for hash collision
            if (existingTheme != theme.AssetName)
            {
                Multiplayer.LogWarning($"Hash collision detected! Theme '{theme.AssetName}' has same hash as '{existingTheme}': {hash}.");
                return 0;
            }
            else
            {
                Multiplayer.LogWarning($"Theme '{theme.AssetName}' is already registered with Id: {hash}.");
                return hash;
            }
        }

        hashToThemeName[hash] = theme.AssetName;
        themeNameToHash[theme.AssetName] = hash;
        Multiplayer.Log($"PaintTheme '{theme.AssetName}' registered with netId: {hash}.");

        return hash;
    }

    public void UnregisterTheme(PaintTheme theme)
    {
        if (theme == null || string.IsNullOrEmpty(theme.AssetName))
            return;

        var hash = GetThemeId(theme.AssetName);

        if (hashToBaseThemeName.ContainsKey(hash))
        {
            Multiplayer.LogWarning($"Tried to unregister a base-game theme '{theme.AssetName}'.");
            return;
        }

        if (hashToThemeName.Remove(hash))
            themeNameToHash.Remove(theme.AssetName);
        else
            Multiplayer.LogWarning($"Tried to unregister theme '{theme.AssetName}', but theme is not registered.");
    }
    #endregion

    #region Helper Methods

    private uint GetThemeId(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
            return 0;

        return StringHashing.Fnv1aHash(themeName);
    }

    private void FindModdedThemes()
    {
        if (moddedThemesSearched)
            return;

        // Find all themes excluding base themes and register non-base themes
        var themes = Object.FindObjectsOfType<PaintTheme>();

        foreach (var theme in themes)
        {
            if (theme != null &&
                !string.IsNullOrEmpty(theme.AssetName) &&
                !baseThemeNamesSet.Contains(theme.AssetName))
            {
                RegisterTheme(theme);
            }
        }

        moddedThemesSearched = true;
    }

    #endregion
}
