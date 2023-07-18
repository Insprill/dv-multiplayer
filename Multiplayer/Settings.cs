using System;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer;

[Serializable]
[DrawFields(DrawFieldMask.OnlyDrawAttr)]
public class Settings : UnityModManager.ModSettings, IDrawable
{
    public static Action<Settings> OnSettingsUpdated;

    [Header("Player")]
    [Draw("Username", Tooltip = "Your username in-game")]
    public string Username = "Player";

    [Header("Server")]
    [Draw("Password", Tooltip = "The password required to join your server. Leave blank for no password.")]
    public string Password;
    [Draw("Max Players", Tooltip = "The maximum number of players that can join your server, including yourself.")]
    public int MaxPlayers = 4;
    [Draw("Port", Tooltip = "The port that your server will listen on. You generally don't need to change this.")]
    public int Port = 7777;

    [Header("Advanced Settings")]
    [Draw("Show Advanced Settings", Tooltip = "You probably don't need to change these.")]
    public bool ShowAdvancedSettings;
    [Draw("Verbose Logging", Tooltip = "Whether to log extra information. This is useful for debugging, but should otherwise be kept off.", VisibleOn = "ShowAdvancedSettings|true")]
    public bool VerboseLogging;
    [Draw("Enable NAT Punch", VisibleOn = "ShowAdvancedSettings|true")]
    public bool EnableNatPunch = true;
    [Draw("Reuse NetPacketReaders", VisibleOn = "ShowAdvancedSettings|true")]
    public bool ReuseNetPacketReaders = true;
    [Draw("Use Native Sockets", VisibleOn = "ShowAdvancedSettings|true")]
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
