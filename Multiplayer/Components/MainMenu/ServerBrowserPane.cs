using System;
using System.Collections;
using System.Text.RegularExpressions;
using DV.Localization;
using DV.UI;
using DV.UIFramework;
using DV.Util;
using DV.Utils;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using Multiplayer.Networking.Data;



namespace Multiplayer.Components.MainMenu
{
    public class ServerBrowserPane : MonoBehaviour
    {
        // Regular expressions for IP and port validation
        // @formatter:off
        // Patterns from https://ihateregex.io/
        private static readonly Regex IPv4Regex = new Regex(@"(\b25[0-5]|\b2[0-4][0-9]|\b[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}");
        private static readonly Regex IPv6Regex = new Regex(@"(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))");
        private static readonly Regex PortRegex = new Regex(@"^((6553[0-5])|(655[0-2][0-9])|(65[0-4][0-9]{2})|(6[0-4][0-9]{3})|([1-5][0-9]{4})|([0-5]{0,5})|([0-9]{1,4}))$");
        // @formatter:on

        

        //Gridview variables
        private ObservableCollectionExt<IServerBrowserGameDetails> gridViewModel = new ObservableCollectionExt<IServerBrowserGameDetails>();
        private ServerBrowserGridView gridView;
        private ScrollRect parentScroller;
        private string serverIDOnRefresh;
        private IServerBrowserGameDetails selectedServer;

        //Button variables
        private Button buttonJoin;
        //private Button buttonHost;
        private Button buttonRefresh;
        private Button buttonDirectIP;

        private bool serverRefreshing = false;

        //connection parameters
        private string ipAddress;
        private ushort portNumber;
        string password = null;
        bool direct = false;

        private string[] testNames = new string[] { "ChooChooExpress", "RailwayRascals", "FreightFrenzy", "SteamDream", "DieselDynasty", "CargoKings", "TrackMasters", "RailwayRevolution", "ExpressElders", "IronHorseHeroes", "LocomotiveLegends", "TrainTitans", "HeavyHaulers", "RapidRails", "TimberlineTransport", "CoalCountry", "SilverRailway", "GoldenGauge", "SteelStream", "MountainMoguls", "RailRiders", "TrackTrailblazers", "FreightFanatics", "SteamSensation", "DieselDaredevils", "CargoChampions", "TrackTacticians", "RailwayRoyals", "ExpressExperts", "IronHorseInnovators", "LocomotiveLeaders", "TrainTacticians", "HeavyHitters", "RapidRunners", "TimberlineTrains", "CoalCrushers", "SilverStreamliners", "GoldenGears", "SteelSurge", "MountainMovers", "RailwayWarriors", "TrackTerminators", "FreightFighters", "SteamStreak", "DieselDynamos", "CargoCommanders", "TrackTrailblazers", "RailwayRangers", "ExpressEngineers", "IronHorseInnovators", "LocomotiveLovers", "TrainTrailblazers", "HeavyHaulersHub", "RapidRailsRacers", "TimberlineTrackers", "CoalCountryCarriers", "SilverSpeedsters", "GoldenGaugeGang", "SteelStalwarts", "MountainMoversClub", "RailRunners", "TrackTitans", "FreightFalcons", "SteamSprinters", "DieselDukes", "CargoCommandos", "TrackTracers", "RailwayRebels", "ExpressElite", "IronHorseIcons", "LocomotiveLunatics", "TrainTornadoes", "HeavyHaulersCrew", "RapidRailsRunners", "TimberlineTrackMasters", "CoalCountryCrew", "SilverSprinters", "GoldenGale", "SteelSpeedsters", "MountainMarauders", "RailwayRiders", "TrackTactics", "FreightFury", "SteamSquires", "DieselDefenders", "CargoCrusaders", "TrackTechnicians", "RailwayRaiders", "ExpressEnthusiasts", "IronHorseIlluminati", "LocomotiveLoyalists", "TrainTurbulence", "HeavyHaulersHeroes", "RapidRailsRiders", "TimberlineTrackTitans", "CoalCountryCaravans", "SilverSpeedRacers", "GoldenGaugeGangsters", "SteelStorm", "MountainMasters", "RailwayRoadrunners", "TrackTerror", "FreightFleets", "SteamSurgeons", "DieselDragons", "CargoCrushers", "TrackTaskmasters", "RailwayRevolutionaries", "ExpressExplorers", "IronHorseInquisitors", "LocomotiveLegion", "TrainTriumph", "HeavyHaulersHorde", "RapidRailsRenegades", "TimberlineTrackTeam", "CoalCountryCrusade", "SilverSprintersSquad", "GoldenGaugeGroup", "SteelStrike", "MountainMonarchs", "RailwayRaid", "TrackTacticiansTeam", "FreightForce", "SteamSquad", "DieselDynastyClan", "CargoCrew", "TrackTeam", "RailwayRalliers", "ExpressExpedition", "IronHorseInitiative", "LocomotiveLeague", "TrainTribe", "HeavyHaulersHustle", "RapidRailsRevolution", "TimberlineTrackersTeam", "CoalCountryConvoy", "SilverSprint", "GoldenGaugeGuild", "SteelSpirits", "MountainMayhem", "RailwayRaidersCrew", "TrackTrailblazersTribe", "FreightFleetForce", "SteamStalwarts", "DieselDragonsDen", "CargoCaptains", "TrackTrailblazersTeam", "RailwayRidersRevolution", "ExpressEliteExpedition", "IronHorseInsiders", "LocomotiveLords", "TrainTacticiansTribe", "HeavyHaulersHeroesHorde", "RapidRailsRacersTeam", "TimberlineTrackMastersTeam", "CoalCountryCarriersCrew", "SilverSpeedstersSprint", "GoldenGaugeGangGuild", "SteelSurgeStrike", "MountainMoversMonarchs" };

        #region setup

        private void Awake()
        {
            Multiplayer.Log("MultiplayerPane Awake()");
            SetupMultiplayerButtons();
            SetupServerBrowser();
            FillDummyServers();
        }

        private void OnEnable()
        {
            Multiplayer.Log("MultiplayerPane OnEnable()");
            if (!this.parentScroller)
            {
                Multiplayer.Log("Find ScrollRect");
                this.parentScroller = this.gridView.GetComponentInParent<ScrollRect>();
                Multiplayer.Log("Found ScrollRect");
            }
            this.SetupListeners(true);
            this.serverIDOnRefresh = "";

            buttonDirectIP.interactable = true;
            buttonRefresh.interactable = true;
            //buttonHost.interactable = true;

        }

        // Disable listeners
        private void OnDisable()
        {
            this.SetupListeners(false);
        }

        private void SetupMultiplayerButtons()
        {
            GameObject goDirectIP = GameObject.Find("ButtonTextIcon Manual");
            //GameObject goHost = GameObject.Find("ButtonTextIcon Host");
            GameObject goJoin = GameObject.Find("ButtonTextIcon Join");
            GameObject goRefresh = GameObject.Find("ButtonIcon Refresh");

            if (goDirectIP == null || /*goHost == null ||*/ goJoin == null || goRefresh == null)
            {
                Multiplayer.LogError("One or more buttons not found.");
                return;
            }

            // Modify the existing buttons' properties
            ModifyButton(goDirectIP, Locale.SERVER_BROWSER__MANUAL_CONNECT_KEY);
            //ModifyButton(goHost, Locale.SERVER_BROWSER__HOST_KEY);
            ModifyButton(goJoin, Locale.SERVER_BROWSER__JOIN_KEY);


            // Set up event listeners and localization for DirectIP button
            buttonDirectIP = goDirectIP.GetComponent<ButtonDV>();
            buttonDirectIP.onClick.AddListener(DirectAction);

            // Set up event listeners and localization for Join button
            buttonJoin = goJoin.GetComponent<ButtonDV>();
            buttonJoin.onClick.AddListener(JoinAction);

            // Set up event listeners and localization for Refresh button
            buttonRefresh = goRefresh.GetComponent<ButtonDV>();
            buttonRefresh.onClick.AddListener(RefreshAction);

            goDirectIP.SetActive(true);
            //goHost.SetActive(true);
            goJoin.SetActive(true);
            goRefresh.SetActive(true);

            buttonJoin.interactable = false;

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
        private void SetupListeners(bool on)
        {
            if (on)
            {
                this.gridView.SelectedIndexChanged += this.IndexChanged;
            }
            else
            {
                this.gridView.SelectedIndexChanged -= this.IndexChanged;
            }

        }

        private void ModifyButton(GameObject button, string key)
        {
            button.GetComponentInChildren<Localize>().key = key;

        }
        private GameObject FindButton(string name)
        {

            return GameObject.Find(name);
        }

        #endregion

        #region UI callbacks

        private void RefreshAction()
        {
            if (serverRefreshing)
                return;

            serverRefreshing = true;
            buttonJoin.interactable = false;

            if (selectedServer != null)
            {
                serverIDOnRefresh = selectedServer.id;
            }

            StartCoroutine(GetRequest($"{Multiplayer.Settings.LobbyServerAddress}/list_game_servers"));

        }
        private void JoinAction()
        {
            if (selectedServer != null)
            {
                buttonDirectIP.interactable = false;
                buttonJoin.interactable = false;
                //buttonHost.interactable = false;

                if (selectedServer.HasPassword)
                {
                    //not making a direct connection
                    direct = false;
                    ipAddress = selectedServer.ip;
                    portNumber = selectedServer.port;

                    ShowPasswordPopup();

                    return;
                }

                SingletonBehaviour<NetworkLifecycle>.Instance.StartClient(selectedServer.ip, selectedServer.port, null);
            }
        }

        private void DirectAction()
        {
            Debug.Log($"DirectAction()");
            buttonDirectIP.interactable = false;
            buttonJoin.interactable = false;
            //buttonHost.interactable = false;

            //making a direct connection
            direct = true;

            ShowIpPopup();
        }

        private void IndexChanged(AGridView<IServerBrowserGameDetails> gridView)
        {
            Debug.Log($"Index: {gridView.SelectedModelIndex}");
            if (serverRefreshing)
                return;

            if (gridView.SelectedModelIndex >= 0)
            {
                Debug.Log($"Selected server: {gridViewModel[gridView.SelectedModelIndex].Name}");

                selectedServer = gridViewModel[gridView.SelectedModelIndex];
                buttonJoin.interactable = true;
            }
            else
            {
                buttonJoin.interactable = false;
            }
        }

        #endregion

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
                    return;

                if (!IPv4Regex.IsMatch(result.data) && !IPv6Regex.IsMatch(result.data))
                {
                    ShowOkPopup(Locale.SERVER_BROWSER__IP_INVALID, ShowIpPopup);
                }
                else
                {
                    ipAddress = result.data;
                    ShowPortPopup();
                }
            };
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
            popup.GetComponentInChildren<TMP_InputField>().text = $"{Multiplayer.Settings.LastRemotePort}";

            popup.Closed += result =>
            {
                if (result.closedBy == PopupClosedByAction.Abortion)
                    return;

                if (!PortRegex.IsMatch(result.data))
                {
                    ShowOkPopup(Locale.SERVER_BROWSER__PORT_INVALID, ShowIpPopup);
                }
                else
                {
                    portNumber = ushort.Parse(result.data);
                    ShowPasswordPopup();
                }
            };

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

            //direct IP connection
            if (direct)
            {
                //Prefill with stored password
                popup.GetComponentInChildren<TMP_InputField>().text = Multiplayer.Settings.LastRemotePassword;

                //Set us up to allow a blank password
                DestroyImmediate(popup.GetComponentInChildren<PopupTextInputFieldController>());
                popup.GetOrAddComponent<PopupTextInputFieldControllerNoValidation>();
             }

            popup.Closed += result =>
            {
                if (result.closedBy == PopupClosedByAction.Abortion)
                    return;

                if (direct)
                {
                    //store params for later
                    Multiplayer.Settings.LastRemoteIP = ipAddress;
                    Multiplayer.Settings.LastRemotePort = portNumber;
                    Multiplayer.Settings.LastRemotePassword = result.data;

                }

                SingletonBehaviour<NetworkLifecycle>.Instance.StartClient(ipAddress, portNumber, result.data);

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



        IEnumerator GetRequest(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    ServerData[] response;

                    response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerData[]>(webRequest.downloadHandler.text);

                    Debug.Log($"servers: {response.Length}");

                    foreach (ServerData server in response)
                    {
                        Debug.Log($"Name: {server.Name}\tIP: {server.ip}");
                    }

                    gridViewModel.Clear();
                    gridView.SetModel(gridViewModel);
                    gridViewModel.AddRange(response);

                    //if we have a server selected, we need to re-select it after refresh
                    if (serverIDOnRefresh != null)
                    {
                        int selID = Array.FindIndex(gridViewModel.ToArray(), server => server.id == serverIDOnRefresh);
                        if (selID >= 0)
                        {
                            gridView.SetSelected(selID);

                            if (this.parentScroller)
                            {
                                this.parentScroller.verticalNormalizedPosition = 1f - (float)selID / (float)gridView.Model.Count;
                            }
                        }

                        serverIDOnRefresh = null;
                    }

                    serverRefreshing = false;
                }
            }
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

        private void FillDummyServers()
        {
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
    }

   
}
