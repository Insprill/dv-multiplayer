using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityModManagerNet;

namespace Multiplayer.Networking.Data;

[Serializable]
public readonly struct ModInfo
{
    public readonly string Id;
    public readonly string Version;
    public readonly string Url;

    public ModInfo(string id, string version, string url)
    {
        Id = id;
        Version = version;

        if (IsTrustedURL(url))
            Url = url;
        else
            Url = "";
    }

    public override string ToString()
    {
        return $"{Id} v{Version}";
    }

    public static void Serialize(NetDataWriter writer, ModInfo modInfo)
    {
        writer.Put(modInfo.Id);
        writer.Put(modInfo.Version);
        writer.Put(modInfo.Url);
    }

    public static ModInfo Deserialize(NetDataReader reader)
    {
        string id = reader.GetString();
        string version = reader.GetString();
        string url = "";

        if (reader.AvailableBytes > 0)
            url = reader.GetString();

        return new ModInfo(id, version, url);
    }

    public static ModInfo[] FromModEntries(IEnumerable<UnityModManager.ModEntry> modEntries)
    {
        return modEntries
            .Where(entry => entry.Enabled)  //We only care if it's enabled
            .OrderBy(entry => entry.Info.Id)
            .Select(entry => new ModInfo(entry.Info.Id, entry.Info.Version, entry.Info?.HomePage))
            .ToArray();
    }

    private static bool IsTrustedURL(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            return false;

        var host = uriResult.Host.ToLowerInvariant();

        if (host == "nexusmods.com" || host == "www.nexusmods.com")
        {
            Multiplayer.LogDebug(() => $"IsTrustedURL() \"{url}\" is Nexus Mods");
            return true;
        }

        if (host == "github.com" || host == "www.github.com")
        {
            Multiplayer.LogDebug(() => $"IsTrustedURL() \"{url}\" is Github");
            return true;
        }

        Multiplayer.LogDebug(() => $"IsTrustedURL() \"{url}\" is untrusted");
        return false;
    }

    public static ModInfo[] DeserializeRequiredMods(string json)
    {
        // Handle null or empty for backward compatibility
        if (string.IsNullOrEmpty(json))
        {
            Multiplayer.LogWarning("No mod data received (likely from older client/server version)");
            return [];
        }

        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ModInfo[]>(json);
        }
        catch (Exception e)
        {
            // Try legacy format: comma-separated string of mod names
            var modNames = json.Split(',')
                .Select(m => m.Trim())
                .Where(m => !string.IsNullOrEmpty(m))
                .Select(m => new ModInfo(m, "Unknown", ""))
                .ToArray();

            return modNames;
        }
    }
}
