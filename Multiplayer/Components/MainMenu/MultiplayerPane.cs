using System;
using System.Text.RegularExpressions;
using DV.Common;
using DV.Localization;
using DV.UI;
using DV.UIFramework;
using DV.Util;
using DV.Utils;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using TMPro;
using UnityEngine;

namespace Multiplayer.Components.MainMenu
{
    public class MultiplayerPane : MonoBehaviour
    {
        // Regular expressions for IP and port validation
        private static readonly Regex IPv4Regex = new Regex(@"(\b25[0-5]|\b2[0-4][0-9]|\b[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}");
        private static readonly Regex IPv6Regex = new Regex(@"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
        private static readonly Regex PortRegex = new Regex(@"^((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9]{2})|(6[0-4][0-9]{3})|([1-5][0-9]{4})|([0-5]{0,5})|([0-9]{1,4}))$");

        private string ipAddress;
        private ushort portNumber;
        //private ButtonDV directButton;

        private ObservableCollectionExt<IServerBrowserGameDetails> gridViewModel = new ObservableCollectionExt<IServerBrowserGameDetails>();
        private ServerBrowserGridView gridView;

        private void Awake()
        {
            Multiplayer.Log("MultiplayerPane Awake()");
            SetupMultiplayerButtons();
            SetupServerBrowser();
        }

        private void SetupMultiplayerButtons()
        {
            GameObject buttonDirectIP = GameObject.Find("ButtonTextIcon Manual");
            GameObject buttonHost = GameObject.Find("ButtonTextIcon Host");
            GameObject buttonJoin = GameObject.Find("ButtonTextIcon Join");
            GameObject buttonRefresh = GameObject.Find("ButtonTextIcon Refresh");

            if (buttonDirectIP == null || buttonHost == null || buttonJoin == null || buttonRefresh == null)
            {
                Multiplayer.LogError("One or more buttons not found.");
                return;
            }

            // Modify the existing buttons' properties
            ModifyButton(buttonDirectIP, Locale.SERVER_BROWSER__MANUAL_CONNECT_KEY);
            ModifyButton(buttonHost, Locale.SERVER_BROWSER__HOST_KEY);
            ModifyButton(buttonJoin, Locale.SERVER_BROWSER__JOIN_KEY);
            //ModifyButton(buttonRefresh, Locale.SERVER_BROWSER__REFRESH);

            // Set up event listeners and localization for DirectIP button
            ButtonDV buttonDirectIPDV = buttonDirectIP.GetComponent<ButtonDV>();
            buttonDirectIPDV.onClick.AddListener(ShowIpPopup);

            // Set up event listeners and localization for Host button
            ButtonDV buttonHostDV = buttonHost.GetComponent<ButtonDV>();
            buttonHostDV.onClick.AddListener(HostAction);

            // Set up event listeners and localization for Join button
            ButtonDV buttonJoinDV = buttonJoin.GetComponent<ButtonDV>();
            buttonJoinDV.onClick.AddListener(JoinAction);

            // Set up event listeners and localization for Refresh button
            //ButtonDV buttonRefreshDV = buttonRefresh.GetComponent<ButtonDV>();
            //buttonRefreshDV.onClick.AddListener(RefreshAction);

            //Debug.Log("Setting buttons active: " + buttonDirectIP.name + ", " + buttonHost.name + ", " + buttonJoin.name + ", " + buttonRefresh.name );
            Debug.Log("Setting buttons active: " + buttonDirectIP.name + ", " + buttonHost.name + ", " + buttonJoin.name );
            buttonDirectIP.SetActive(true);
            buttonHost.SetActive(true);
            buttonJoin.SetActive(true);
            //buttonRefresh.SetActive(true);
        }

        private void SetupServerBrowser()
        {
            /*GameObject.Destroy(this.FindChildByName("GRID VIEW"));
            GameObject Viewport = GameObject.Find("Viewport");

            GameObject serverBrowserGridView = new GameObject("GRID VIEW", typeof (ServerBrowserGridView));
            serverBrowserGridView.transform.SetParent(Viewport.transform);
            gridView = serverBrowserGridView.GetComponent<ServerBrowserGridView>();
            Debug.Log("found Grid View");

            RectTransform rt = serverBrowserGridView.GetComponent<RectTransform>();
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 5292);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 662);
            */
            GameObject GridviewGO = this.FindChildByName("GRID VIEW");
            SaveLoadGridView slgv = GridviewGO.GetComponent<SaveLoadGridView>();
            GridviewGO.SetActive(false);
            
            gridView = GridviewGO.AddComponent<ServerBrowserGridView>();
            gridView.dummyElementPrefab = Instantiate(slgv.viewElementPrefab);
            gridView.dummyElementPrefab.name = "prefabServerBrowser";
            GameObject.Destroy(slgv);
            GridviewGO.SetActive(true);

            
            //gridView.dummyElementPrefab = null;
            //gridViewModel.Add();



        }

        private GameObject FindButton(string name)
        {

            return GameObject.Find(name);
        }

        private void ModifyButton(GameObject button, string key)
        {
            button.GetComponentInChildren<Localize>().key = key;

        }

        private void ShowIpPopup()
        {
            Debug.Log("In ShowIpPpopup");
            var popup = MainMenuThingsAndStuff.Instance.ShowRenamePopup();
            if (popup == null)
            {
                Multiplayer.LogError("Popup not found.");
                return;
            }

            popup.labelTMPro.text = Locale.SERVER_BROWSER__IP;
            popup.GetComponentInChildren<TMP_InputField>().text = Multiplayer.Settings.LastRemoteIP;

            popup.Closed += result =>
            {
                if (result.closedBy == PopupClosedByAction.Abortion)
                {
                    MainMenuThingsAndStuff.Instance.SwitchToDefaultMenu();
                    return;
                }

                HandleIpAddressInput(result.data);
            };
        }

        private void HandleIpAddressInput(string input)
        {
            if (!IPv4Regex.IsMatch(input) && !IPv6Regex.IsMatch(input))
            {
                ShowOkPopup(Locale.SERVER_BROWSER__IP_INVALID, ShowIpPopup);
                return;
            }

            ipAddress = input;
            ShowPortPopup();
        }

        private void ShowPortPopup()
        {
            var popup = MainMenuThingsAndStuff.Instance.ShowRenamePopup();
            if (popup == null)
            {
                Multiplayer.LogError("Popup not found.");
                return;
            }

            popup.labelTMPro.text = Locale.SERVER_BROWSER__PORT;
            popup.GetComponentInChildren<TMP_InputField>().text = Multiplayer.Settings.LastRemotePort.ToString();

            popup.Closed += result =>
            {
                if (result.closedBy == PopupClosedByAction.Abortion)
                {
                    MainMenuThingsAndStuff.Instance.SwitchToDefaultMenu();
                    return;
                }

                HandlePortInput(result.data);
            };
        }

        private void HandlePortInput(string input)
        {
            if (!PortRegex.IsMatch(input))
            {
                ShowOkPopup(Locale.SERVER_BROWSER__PORT_INVALID, ShowPortPopup);
                return;
            }

            portNumber = ushort.Parse(input);
            ShowPasswordPopup();
        }

        private void ShowPasswordPopup()
        {
            var popup = MainMenuThingsAndStuff.Instance.ShowRenamePopup();
            if (popup == null)
            {
                Multiplayer.LogError("Popup not found.");
                return;
            }

            popup.labelTMPro.text = Locale.SERVER_BROWSER__PASSWORD;
            popup.GetComponentInChildren<TMP_InputField>().text = Multiplayer.Settings.LastRemotePassword;

            DestroyImmediate(popup.GetComponentInChildren<PopupTextInputFieldController>());
            popup.GetOrAddComponent<PopupTextInputFieldControllerNoValidation>();

            popup.Closed += result =>
            {
                if (result.closedBy == PopupClosedByAction.Abortion) return;

                //directButton.enabled = false;
                SingletonBehaviour<NetworkLifecycle>.Instance.StartClient(ipAddress, portNumber, result.data);

                Multiplayer.Settings.LastRemoteIP = ipAddress;
                Multiplayer.Settings.LastRemotePort = portNumber;
                Multiplayer.Settings.LastRemotePassword = result.data;

                //ShowConnectingPopup(); // Show a connecting message
                //SingletonBehaviour<NetworkLifecycle>.Instance.ConnectionFailed += HandleConnectionFailed;
                //SingletonBehaviour<NetworkLifecycle>.Instance.ConnectionEstablished += HandleConnectionEstablished;
            };
        }

        // Example of handling connection success
        private void HandleConnectionEstablished()
        {
            // Connection established, handle the UI or game state accordingly
            Debug.Log("Connection established!");
            // HideConnectingPopup(); // Hide the connecting message
        }

        // Example of handling connection failure
        private void HandleConnectionFailed()
        {
            // Connection failed, show an error message or handle the failure scenario
            Debug.LogError("Connection failed!");
            // ShowConnectionFailedPopup();
        }

        private void RefreshAction()
        {
            // Implement refresh action logic here
            Debug.Log("Refresh button clicked.");
            // Add your code to refresh the multiplayer list or perform any other refresh-related action
        }


        private static void ShowOkPopup(string text, Action onClick)
        {
            var popup = MainMenuThingsAndStuff.Instance.ShowOkPopup();
            if (popup == null) return;

            popup.labelTMPro.text = text;
            popup.Closed += _ => onClick();
        }

        private void SetButtonsActive(params GameObject[] buttons)
        {
            foreach (var button in buttons)
            {
                button.SetActive(true);
            }
        }

        private void HostAction()
        {
            // Implement host action logic here
            Debug.Log("Host button clicked.");
            // Add your code to handle hosting a game
            gridView.showDummyElement = true;
            gridViewModel.Clear();
            //gridView.dummyElementPrefab = ;

            Debug.Log($"gridViewPrefab exists : {gridView.dummyElementPrefab != null} showDummyElement : {gridView.showDummyElement}");
            gridView.SetModel(gridViewModel);

        }

        private void JoinAction()
        {
            // Implement join action logic here
            Debug.Log("Join button clicked.");
            // Add your code to handle joining a game
        }
    }
}
