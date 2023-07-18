using System;
using System.Text.RegularExpressions;
using DV.UI;
using DV.UIFramework;
using DV.Utils;
using Multiplayer.Components.Networking;
using UnityEngine;

namespace Multiplayer.Components.MainMenu;

public class MultiplayerPane : MonoBehaviour
{
    // @formatter:off
    // Patterns from https://ihateregex.io/
    private static readonly Regex IPv4 = new(@"(\b25[0-5]|\b2[0-4][0-9]|\b[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}");
    private static readonly Regex IPv6 = new(@"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
    private static readonly Regex PORT = new(@"^((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9]{2})|(6[0-4][0-9]{3})|([1-5][0-9]{4})|([0-5]{0,5})|([0-9]{1,4}))$");
    // @formatter:on

    private bool why;
    private PopupManager popupManager;
    public Popup renamePopupPrefab;
    public Popup okPopupPrefab;
    public UIMenuController uiMenuController;

    private string address;
    private ushort port;

    private void Awake()
    {
        bool shouldDestroy = false;

        this.FindPopupManager(ref popupManager);

        if (popupManager == null)
        {
            Multiplayer.LogError("Failed to find PopupManager! Destroying self.");
            shouldDestroy = true;
        }

        if (renamePopupPrefab == null)
        {
            Multiplayer.LogError($"{nameof(renamePopupPrefab)} is null! Destroying self.");
            shouldDestroy = true;
        }

        if (okPopupPrefab == null)
        {
            Multiplayer.LogError($"{nameof(okPopupPrefab)} is null! Destroying self.");
            shouldDestroy = true;
        }

        if (uiMenuController == null)
        {
            Multiplayer.LogError($"{nameof(uiMenuController)} is null! Destroying self.");
            shouldDestroy = true;
        }

        if (!shouldDestroy)
            return;

        Destroy(this);
    }

    private void OnEnable()
    {
        if (!why)
        {
            why = true;
            return;
        }

        ShowIpPopup();
    }

    private void ShowIpPopup()
    {
        if (!CanShowPopup())
            return;

        Popup popup = popupManager.ShowPopup(renamePopupPrefab);
        popup.labelTMPro.text = "Enter IP Address";

        popup.Closed += result =>
        {
            if (result.closedBy == PopupClosedByAction.Abortion)
            {
                uiMenuController.SwitchMenuTask(uiMenuController.defaultMenuIndex);
                return;
            }

            if (!IPv4.IsMatch(result.data) && !IPv6.IsMatch(result.data))
            {
                ShowOkPopup("Invalid IP Address!", ShowIpPopup);
                return;
            }

            address = result.data;

            ShowPortPopup();
        };
    }

    private void ShowPortPopup()
    {
        if (!CanShowPopup())
            return;

        Popup popup = popupManager.ShowPopup(renamePopupPrefab);
        popup.labelTMPro.text = "Enter the port (7777 by default)";

        popup.Closed += result =>
        {
            if (result.closedBy == PopupClosedByAction.Abortion)
            {
                uiMenuController.SwitchMenuTask(uiMenuController.defaultMenuIndex);
                return;
            }

            if (!PORT.IsMatch(result.data))
            {
                ShowOkPopup("Invalid Port!", ShowPortPopup);
                return;
            }

            port = ushort.Parse(result.data);

            ShowPasswordPopup();
        };
    }

    private void ShowPasswordPopup()
    {
        if (!CanShowPopup())
            return;

        Popup popup = popupManager.ShowPopup(renamePopupPrefab);
        popup.labelTMPro.text = "Enter the password";

        popup.Closed += result =>
        {
            if (result.closedBy == PopupClosedByAction.Abortion)
            {
                uiMenuController.SwitchMenuTask(uiMenuController.defaultMenuIndex);
                return;
            }

            SingletonBehaviour<NetworkLifecycle>.Instance.StartClient(address, port, result.data);
        };
    }

    private void ShowOkPopup(string text, Action onClick)
    {
        if (!CanShowPopup())
            return;

        Popup popup = popupManager.ShowPopup(okPopupPrefab);
        popup.labelTMPro.text = text;
        popup.Closed += _ => { onClick(); };
    }

    private bool CanShowPopup()
    {
        if (popupManager.CanShowPopup())
            return true;
        Multiplayer.LogError("PopupManager cannot show popup!");
        return false;
    }
}
