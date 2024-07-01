using System;
using Multiplayer.Networking.Data;
using Multiplayer.Networking.Listeners;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Multiplayer.Components.Networking;
using DV.WeatherSystem;
using DV.UserManagement;

namespace Multiplayer.Networking.Managers.Server;
public class LobbyServerManager : MonoBehaviour
{
    private const int UPDATE_TIME_BUFFER = 10;
    private const int UPDATE_TIME = 120 - UPDATE_TIME_BUFFER; //how often to update the lobby server
    private const int PLAYER_CHANGE_TIME = 5; //update server early if the number of players has changed in this time frame

    private NetworkServer server;
    public string server_id { get; set; }
    public string private_key { get; set; }

    private bool sendUpdates = false;


    private float timePassed = 0f;

    private void Awake()
    {
        this.server = NetworkLifecycle.Instance.Server;

        Debug.Log($"LobbyServerManager New({server != null})");
        Debug.Log($"StartingCoroutine {Multiplayer.Settings.LobbyServerAddress}/add_game_server\")");
        StartCoroutine(this.RegisterWithLobbyServer($"{Multiplayer.Settings.LobbyServerAddress}/add_game_server"));
    }

    private void OnDestroy()
    {
        Debug.Log($"LobbyServerManager OnDestroy()");
        sendUpdates = false;
        this.StopAllCoroutines();
        StartCoroutine(this.RemoveFromLobbyServer($"{Multiplayer.Settings.LobbyServerAddress}/remove_game_server"));
    }

    private void Update()
    {
        if (sendUpdates)
        {
            timePassed += Time.deltaTime;

            if(timePassed > UPDATE_TIME || (server.serverData.CurrentPlayers != server.PlayerCount && timePassed > PLAYER_CHANGE_TIME)){
                timePassed = 0f;
                server.serverData.CurrentPlayers = server.PlayerCount;
                StartCoroutine(this.UpdateLobbyServer($"{Multiplayer.Settings.LobbyServerAddress}/update_game_server"));
            }
        }
    }
    public void RemoveFromLobbyServer()
    {
        Debug.Log($"RemoveFromLobbyServer OnDestroy()");
        sendUpdates = false;
        this.StopAllCoroutines();
        StartCoroutine(this.RemoveFromLobbyServer($"{Multiplayer.Settings.LobbyServerAddress}/remove_game_server"));
    }


    IEnumerator RegisterWithLobbyServer(string uri)
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        jsonSettings.NullValueHandling = NullValueHandling.Ignore;

        string json = JsonConvert.SerializeObject(server.serverData, jsonSettings);
        Debug.Log($"JsonRequest: {json}");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, json))
        {
            UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            customUploadHandler.contentType = "application/json";
            webRequest.uploadHandler = customUploadHandler;

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error + "\r\n" + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                LobbyServerResponseData response;

                response = JsonConvert.DeserializeObject<LobbyServerResponseData>(webRequest.downloadHandler.text);

                if (response != null)
                {
                    this.private_key = response.private_key;
                    this.server_id = response.game_server_id;
                    this.sendUpdates = true;
                }
            }
        }
    }

    IEnumerator RemoveFromLobbyServer(string uri)
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        jsonSettings.NullValueHandling = NullValueHandling.Ignore;

        string json = JsonConvert.SerializeObject(new LobbyServerResponseData(this.server_id, this.private_key), jsonSettings);
        Debug.Log($"JsonRequest: {json}");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, json))
        {
            UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            customUploadHandler.contentType = "application/json";
            webRequest.uploadHandler = customUploadHandler;

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error + "\r\n" + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
            }
        }
    }

    IEnumerator UpdateLobbyServer(string uri)
    {
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        jsonSettings.NullValueHandling = NullValueHandling.Ignore;

        DateTime start = AStartGameData.BaseTimeAndDate;
        DateTime current = WeatherDriver.Instance.manager.DateTime;

        TimeSpan inGame = current - start;
        

        string json = JsonConvert.SerializeObject(new LobbyServerUpdateData(this.server_id, this.private_key, inGame.ToString("d\\d\\ hh\\h\\ mm\\m\\ ss\\s"), server.serverData.CurrentPlayers), jsonSettings);
        Debug.Log($"UpdateLobbyServer JsonRequest: {json}");

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, json))
        {
            UploadHandler customUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            customUploadHandler.contentType = "application/json";
            webRequest.uploadHandler = customUploadHandler;

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error + "\r\n" + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
            }
        }
    }
}
