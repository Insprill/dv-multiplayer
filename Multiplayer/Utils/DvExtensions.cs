using System;
using DV.UI;
using DV.UIFramework;
using DV.Localization;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Components.Networking.World;
using UnityEngine;
using UnityEngine.UI;



namespace Multiplayer.Utils;

public static class DvExtensions
{
    #region TrainCar

    public static ushort GetNetId(this TrainCar car)
    {
        ushort netId = car.Networked().NetId;
        if (netId == 0)
            throw new InvalidOperationException($"NetId for {car.carLivery.id} ({car.ID}) isn't initialized!");
        return netId;
    }

    public static NetworkedTrainCar Networked(this TrainCar trainCar)
    {
        return NetworkedTrainCar.GetFromTrainCar(trainCar);
    }

    public static bool TryNetworked(this TrainCar trainCar, out NetworkedTrainCar networkedTrainCar)
    {
        return NetworkedTrainCar.TryGetFromTrainCar(trainCar, out networkedTrainCar);
    }

    #endregion

    #region RailTrack

    public static NetworkedRailTrack Networked(this RailTrack railTrack)
    {
        return NetworkedRailTrack.GetFromRailTrack(railTrack);
    }

    #endregion

    #region UI
    public static void UpdateButton(this GameObject pane, string oldButtonName, string newButtonName, string localeKey, string toolTipKey, Sprite icon)
    {
        // Find and rename the button
        GameObject button = pane.FindChildByName(oldButtonName);
        button.name = newButtonName;

        // Update localization and tooltip
        if (button.GetComponentInChildren<Localize>() != null)
        {
            button.GetComponentInChildren<Localize>().key = localeKey;
            GameObject.Destroy(button.GetComponentInChildren<I2.Loc.Localize>());
            ResetTooltip(button);
        }

        // Set the button icon if provided
        if (icon != null)
        {
            SetButtonIcon(button, icon);
        }

        // Enable button interaction
        button.GetComponentInChildren<ButtonDV>().ToggleInteractable(true);
    }

    private static void SetButtonIcon(this GameObject button, Sprite icon)
    {
        // Find and set the icon for the button
        GameObject goIcon = button.FindChildByName("[icon]");
        if (goIcon == null)
        {
            Multiplayer.LogError("Failed to find icon!");
            return;
        }

        goIcon.GetComponent<Image>().sprite = icon;
    }

    private static void ResetTooltip(this GameObject button)
    {
        // Reset the tooltip keys for the button
        UIElementTooltip tooltip = button.GetComponent<UIElementTooltip>();
        tooltip.disabledKey = null;
        tooltip.enabledKey = null;
    }

    #endregion
}
