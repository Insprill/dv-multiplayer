using System;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer;

[Serializable]
public class Settings : UnityModManager.ModSettings, IDrawable
{
    [Header("Player")]
    [Draw("Username")]
    public string Username;

    [Header("Advanced Settings")]
    [Draw("Show Advanced Settings")]
    public bool ShowAdvancedSettings;
    [Draw("Verbose Logging", VisibleOn = "ShowAdvancedSettings")]
    public bool VerboseLogging;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }

    public void OnChange()
    {
        // yup
    }
}
