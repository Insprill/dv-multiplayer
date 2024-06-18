using System;
using System.Collections.Generic;
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
using UnityEngine.Events;
using UnityEngine.UI;

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
        private ScrollRect parentScroller;
        private int indexToSelectOnRefresh;

        private string[] testNames = new string[] { "ChooChooExpress", "RailwayRascals", "FreightFrenzy", "SteamDream", "DieselDynasty", "CargoKings", "TrackMasters", "RailwayRevolution", "ExpressElders", "IronHorseHeroes", "LocomotiveLegends", "TrainTitans", "HeavyHaulers", "RapidRails", "TimberlineTransport", "CoalCountry", "SilverRailway", "GoldenGauge", "SteelStream", "MountainMoguls", "RailRiders", "TrackTrailblazers", "FreightFanatics", "SteamSensation", "DieselDaredevils", "CargoChampions", "TrackTacticians", "RailwayRoyals", "ExpressExperts", "IronHorseInnovators", "LocomotiveLeaders", "TrainTacticians", "HeavyHitters", "RapidRunners", "TimberlineTrains", "CoalCrushers", "SilverStreamliners", "GoldenGears", "SteelSurge", "MountainMovers", "RailwayWarriors", "TrackTerminators", "FreightFighters", "SteamStreak", "DieselDynamos", "CargoCommanders", "TrackTrailblazers", "RailwayRangers", "ExpressEngineers", "IronHorseInnovators", "LocomotiveLovers", "TrainTrailblazers", "HeavyHaulersHub", "RapidRailsRacers", "TimberlineTrackers", "CoalCountryCarriers", "SilverSpeedsters", "GoldenGaugeGang", "SteelStalwarts", "MountainMoversClub", "RailRunners", "TrackTitans", "FreightFalcons", "SteamSprinters", "DieselDukes", "CargoCommandos", "TrackTracers", "RailwayRebels", "ExpressElite", "IronHorseIcons", "LocomotiveLunatics", "TrainTornadoes", "HeavyHaulersCrew", "RapidRailsRunners", "TimberlineTrackMasters", "CoalCountryCrew", "SilverSprinters", "GoldenGale", "SteelSpeedsters", "MountainMarauders", "RailwayRiders", "TrackTactics", "FreightFury", "SteamSquires", "DieselDefenders", "CargoCrusaders", "TrackTechnicians", "RailwayRaiders", "ExpressEnthusiasts", "IronHorseIlluminati", "LocomotiveLoyalists", "TrainTurbulence", "HeavyHaulersHeroes", "RapidRailsRiders", "TimberlineTrackTitans", "CoalCountryCaravans", "SilverSpeedRacers", "GoldenGaugeGangsters", "SteelStorm", "MountainMasters", "RailwayRoadrunners", "TrackTerror", "FreightFleets", "SteamSurgeons", "DieselDragons", "CargoCrushers", "TrackTaskmasters", "RailwayRevolutionaries", "ExpressExplorers", "IronHorseInquisitors", "LocomotiveLegion", "TrainTriumph", "HeavyHaulersHorde", "RapidRailsRenegades", "TimberlineTrackTeam", "CoalCountryCrusade", "SilverSprintersSquad", "GoldenGaugeGroup", "SteelStrike", "MountainMonarchs", "RailwayRaid", "TrackTacticiansTeam", "FreightForce", "SteamSquad", "DieselDynastyClan", "CargoCrew", "TrackTeam", "RailwayRalliers", "ExpressExpedition", "IronHorseInitiative", "LocomotiveLeague", "TrainTribe", "HeavyHaulersHustle", "RapidRailsRevolution", "TimberlineTrackersTeam", "CoalCountryConvoy", "SilverSprint", "GoldenGaugeGuild", "SteelSpirits", "MountainMayhem", "RailwayRaidersCrew", "TrackTrailblazersTribe", "FreightFleetForce", "SteamStalwarts", "DieselDragonsDen", "CargoCaptains", "TrackTrailblazersTeam", "RailwayRidersRevolution", "ExpressEliteExpedition", "IronHorseInsiders", "LocomotiveLords", "TrainTacticiansTribe", "HeavyHaulersHeroesHorde", "RapidRailsRacersTeam", "TimberlineTrackMastersTeam", "CoalCountryCarriersCrew", "SilverSpeedstersSprint", "GoldenGaugeGangGuild", "SteelSurgeStrike", "MountainMoversMonarchs" };

        private void Awake()
        {
            Multiplayer.Log("MultiplayerPane Awake()");
            SetupMultiplayerButtons();
            SetupServerBrowser();
        }

        private void OnEnable()
        {
            if (!this.parentScroller)
            {
                this.parentScroller = this.gridView.GetComponentInParent<ScrollRect>();
            }
            this.SetupListeners(true);
            this.indexToSelectOnRefresh = 0;
            this.RefreshData();
        }

        // Token: 0x060001C2 RID: 450 RVA: 0x00007D0C File Offset: 0x00005F0C
        private void OnDisable()
        {
            this.SetupListeners(false);
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
            Debug.Log("Setting buttons active: " + buttonDirectIP.name + ", " + buttonHost.name + ", " + buttonJoin.name);
            buttonDirectIP.SetActive(true);
            buttonHost.SetActive(true);
            buttonJoin.SetActive(true);
            //buttonRefresh.SetActive(true);
        }

        private void SetupServerBrowser()
        {
            GameObject GridviewGO = this.FindChildByName("GRID VIEW");
            SaveLoadGridView slgv = GridviewGO.GetComponent<SaveLoadGridView>();

            GridviewGO.SetActive(false);

            gridView = GridviewGO.AddComponent<ServerBrowserGridView>();
            gridView.dummyElementPrefab = Instantiate(slgv.viewElementPrefab);
            gridView.dummyElementPrefab.name = "prefabServerBrowser";

            GameObject.Destroy(slgv);

            GridviewGO.SetActive(true);
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


            //gridView.showDummyElement = true;
            gridViewModel.Clear();


            IServerBrowserGameDetails item = null;

            for (int i = 0; i < UnityEngine.Random.Range(1, 50); i++)
            {

                item = new ServerData();
                item.Name = testNames[UnityEngine.Random.Range(0, testNames.Length - 1)];
                item.MaxPlayers = UnityEngine.Random.Range(1, 10);
                item.CurrentPlayers = UnityEngine.Random.Range(1, item.MaxPlayers);
                item.Ping = UnityEngine.Random.Range(5, 1500);
                item.HasPassword = UnityEngine.Random.Range(0, 10) > 5;

                Debug.Log(item.HasPassword);
                gridViewModel.Add(item);
            }

            gridView.SetModel(gridViewModel);

        }

        private void JoinAction()
        {
            // Implement join action logic here
            Debug.Log("Join button clicked.");
            // Add your code to handle joining a game
        }
        private void SetupListeners(bool on)
        {
            if (on)
            {
                return;
            }
            
        }
        private void RefreshData()
        {
            
        }
    }

    public class ServerData : IServerBrowserGameDetails
    {
        public int ServerID { get; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public int Ping { get; set; }
        public bool HasPassword { get; set; }

        public void Dispose() { }
    }
}
