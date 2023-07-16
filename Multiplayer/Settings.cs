using System;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer;

[Serializable]
[DrawFields(DrawFieldMask.OnlyDrawAttr)]
public class Settings : UnityModManager.ModSettings, IDrawable
{
    public Action<Settings> OnSettingsUpdated;

    [Header("Player")]
    [Draw("Username")]
    public string Username;

    [Header("Server")]
    [Draw("Password")]
    public string Password;
    [Draw("Max Players")]
    public int MaxPlayers;
    [Draw("Port")]
    public int Port;

    [Header("Advanced Settings")]
    [Draw("Show Advanced Settings")]
    public bool ShowAdvancedSettings;
    [Draw("Verbose Logging", VisibleOn = "ShowAdvancedSettings")]
    public bool VerboseLogging;
    [Draw("Enable NAT Punch", VisibleOn = "ShowAdvancedSettings")]
    public bool EnableNatPunch = true;
    [Draw("Reuse NetPacketReaders", VisibleOn = "ShowAdvancedSettings")]
    public bool ReuseNetPacketReaders = true;
    [Draw("Use Native Sockets", VisibleOn = "ShowAdvancedSettings")]
    public bool UseNativeSockets = true;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Port = Mathf.Clamp(Port, 1024, 49151);
        OnSettingsUpdated?.Invoke(this);
        Save(this, modEntry);
    }

    public void OnChange()
    {
        // yup
    }
}
