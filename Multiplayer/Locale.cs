using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using I2.Loc;
using Multiplayer.Utils;

namespace Multiplayer;

public static class Locale
{
    private const string DEFAULT_LOCALE_FILE = "locale.csv";

    private const string DEFAULT_LANGUAGE = "English";
    public const string MISSING_TRANSLATION = "[ MISSING TRANSLATION ]";
    public const string PREFIX = "multiplayer/";

    private const string PREFIX_MAIN_MENU = $"{PREFIX}mm";
    private const string PREFIX_SERVER_BROWSER = $"{PREFIX}sb";
    private const string PREFIX_DISCONN_REASON = $"{PREFIX}dr";
    private const string PREFIX_CAREER_MANAGER = $"{PREFIX}carman";
    private const string PREFIX_PLAYER_LIST = $"{PREFIX}plist";
    private const string PREFIX_LOADING_INFO = $"{PREFIX}linfo";

    #region Main Menu

    public static string MAIN_MENU__JOIN_SERVER => Get(MAIN_MENU__JOIN_SERVER_KEY);
    public const string MAIN_MENU__JOIN_SERVER_KEY = $"{PREFIX_MAIN_MENU}/join_server";

    #endregion

    #region Server Browser

    public static string SERVER_BROWSER__TITLE => Get(SERVER_BROWSER__TITLE_KEY);
    public const string SERVER_BROWSER__TITLE_KEY = $"{PREFIX_SERVER_BROWSER}/title";

    public static string SERVER_BROWSER__DIRECT => Get(SERVER_BROWSER__DIRECT_KEY);
    public const string SERVER_BROWSER__DIRECT_KEY = $"{PREFIX_SERVER_BROWSER}/direct";

    public static string SERVER_BROWSER__IP => Get(SERVER_BROWSER__IP_KEY);
    private const string SERVER_BROWSER__IP_KEY = $"{PREFIX_SERVER_BROWSER}/ip";
    public static string SERVER_BROWSER__IP_INVALID => Get(SERVER_BROWSER__IP_INVALID_KEY);
    private const string SERVER_BROWSER__IP_INVALID_KEY = $"{PREFIX_SERVER_BROWSER}/ip_invalid";
    public static string SERVER_BROWSER__PORT => Get(SERVER_BROWSER__PORT_KEY);
    private const string SERVER_BROWSER__PORT_KEY = $"{PREFIX_SERVER_BROWSER}/port";
    public static string SERVER_BROWSER__PORT_INVALID => Get(SERVER_BROWSER__PORT_INVALID_KEY);
    private const string SERVER_BROWSER__PORT_INVALID_KEY = $"{PREFIX_SERVER_BROWSER}/port_invalid";
    public static string SERVER_BROWSER__PASSWORD => Get(SERVER_BROWSER__PASSWORD_KEY);
    private const string SERVER_BROWSER__PASSWORD_KEY = $"{PREFIX_SERVER_BROWSER}/password";

    #endregion

    #region Disconnect Reason

    public static string DISCONN_REASON__INVALID_PASSWORD => Get(DISCONN_REASON__INVALID_PASSWORD_KEY);
    public const string DISCONN_REASON__INVALID_PASSWORD_KEY = $"{PREFIX_DISCONN_REASON}/invalid_password";
    public static string DISCONN_REASON__GAME_VERSION => Get(DISCONN_REASON__GAME_VERSION_KEY);
    public const string DISCONN_REASON__GAME_VERSION_KEY = $"{PREFIX_DISCONN_REASON}/game_version";
    public static string DISCONN_REASON__FULL_SERVER => Get(DISCONN_REASON__FULL_SERVER_KEY);
    public const string DISCONN_REASON__FULL_SERVER_KEY = $"{PREFIX_DISCONN_REASON}/full_server";
    public static string DISCONN_REASON__MODS => Get(DISCONN_REASON__MODS_KEY);
    public const string DISCONN_REASON__MODS_KEY = $"{PREFIX_DISCONN_REASON}/mods";
    public static string DISCONN_REASON__MODS_MISSING => Get(DISCONN_REASON__MODS_MISSING_KEY);
    public const string DISCONN_REASON__MODS_MISSING_KEY = $"{PREFIX_DISCONN_REASON}/mods_missing";
    public static string DISCONN_REASON__MODS_EXTRA => Get(DISCONN_REASON__MODS_EXTRA_KEY);
    public const string DISCONN_REASON__MODS_EXTRA_KEY = $"{PREFIX_DISCONN_REASON}/mods_extra";

    #endregion

    #region Career Manager

    public static string CAREER_MANAGER__FEES_HOST_ONLY => Get(CAREER_MANAGER__FEES_HOST_ONLY_KEY);
    private const string CAREER_MANAGER__FEES_HOST_ONLY_KEY = $"{PREFIX_CAREER_MANAGER}/fees_host_only";

    #endregion

    #region Player List

    public static string PLAYER_LIST__TITLE => Get(PLAYER_LIST__TITLE_KEY);
    private const string PLAYER_LIST__TITLE_KEY = $"{PREFIX_PLAYER_LIST}/title";

    #endregion

    #region Loading Info

    public static string LOADING_INFO__WAIT_FOR_SERVER => Get(LOADING_INFO__WAIT_FOR_SERVER_KEY);
    private const string LOADING_INFO__WAIT_FOR_SERVER_KEY = $"{PREFIX_LOADING_INFO}/wait_for_server";

    public static string LOADING_INFO__SYNC_WORLD_STATE => Get(LOADING_INFO__SYNC_WORLD_STATE_KEY);
    private const string LOADING_INFO__SYNC_WORLD_STATE_KEY = $"{PREFIX_LOADING_INFO}/sync_world_state";

    #endregion

    private static bool initializeAttempted;
    private static ReadOnlyDictionary<string, Dictionary<string, string>> csv;

    public static void Load(string localeDir)
    {
        initializeAttempted = true;
        string path = Path.Combine(localeDir, DEFAULT_LOCALE_FILE);
        if (!File.Exists(path))
        {
            Multiplayer.LogError($"Failed to find locale file at '{path}'! Please make sure it's there.");
            return;
        }

        csv = Csv.Parse(File.ReadAllText(path));
        Multiplayer.LogDebug(() => $"Locale dump:{Csv.Dump(csv)}");
    }

    public static string Get(string key, string overrideLanguage = null)
    {
        if (!initializeAttempted)
            throw new InvalidOperationException("Not initialized");

        if (csv == null)
            return MISSING_TRANSLATION;

        string locale = overrideLanguage ?? LocalizationManager.CurrentLanguage;
        if (!csv.ContainsKey(locale))
        {
            if (locale == DEFAULT_LANGUAGE)
            {
                Multiplayer.LogError($"Failed to find locale language {locale}! Something is broken, this shouldn't happen. Dumping CSV data:");
                Multiplayer.LogError($"\n{Csv.Dump(csv)}");
                return MISSING_TRANSLATION;
            }

            locale = DEFAULT_LANGUAGE;
            Multiplayer.LogWarning($"Failed to find locale language {locale}");
        }

        Dictionary<string, string> localeDict = csv[locale];
        string actualKey = key.StartsWith(PREFIX) ? key.Substring(PREFIX.Length) : key;
        if (localeDict.TryGetValue(actualKey, out string value))
        {
            if (value == string.Empty)
                return overrideLanguage == null && locale != DEFAULT_LANGUAGE ? Get(actualKey, DEFAULT_LANGUAGE) : MISSING_TRANSLATION;
            return value;
        }

        Multiplayer.LogDebug(() => $"Failed to find locale key '{actualKey}'!");
        return MISSING_TRANSLATION;
    }

    public static string Get(string key, params object[] placeholders)
    {
        return string.Format(Get(key), placeholders);
    }

    public static string Get(string key, params string[] placeholders)
    {
        // ReSharper disable once CoVariantArrayConversion
        return Get(key, (object[])placeholders);
    }
}
