using System;
using System.Collections;
using System.Collections.Generic;
using DV.Utils;
using LiteNetLib;
using Multiplayer.Networking.Listeners;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer.Components.Networking;

// https://revenantx.github.io/LiteNetLib/index.html
public class NetworkLifecycle : SingletonBehaviour<NetworkLifecycle>
{
    private const byte TICK_RATE = 60;
    private const float TICK_INTERVAL = 1.0f / TICK_RATE;

    public NetworkServer Server { get; private set; }
    public NetworkClient Client { get; private set; }

    public bool IsServerRunning => Server?.IsRunning ?? false;
    public bool IsClientRunning => Client?.IsRunning ?? false;

    public bool IsProcessingPacket => Client.IsProcessingPacket;

    /// <summary>
    ///     Whether the provided NetPeer is the host.
    ///     Note that this does NOT check authority, and should only be used for client-only logic.
    /// </summary>
    public bool IsHost(NetPeer peer)
    {
        return Server?.IsRunning == true && Client?.IsRunning == true && Client?.selfPeer?.Id == peer?.Id;
    }

    /// <summary>
    ///     Whether the caller is the host.
    ///     Note that this does NOT check authority, and should only be used for client-only logic.
    /// </summary>
    public bool IsHost()
    {
        return IsHost(Client?.selfPeer);
    }

    private readonly Queue<Action> mainMenuLoadedQueue = new();

    protected override void Awake()
    {
        base.Awake();
        WorldStreamingInit.LoadingFinished += OnWorldLoaded;
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.buildIndex != (int)DVScenes.MainMenu)
                return;
            TriggerMainMenuEventLater();
        };
        StartCoroutine(PollEvents());
    }

    private static void OnWorldLoaded()
    {
        // World moving is hard-disabled via the WorldMoverPatch, but we update this anyway so scripts are aware of that.
        WorldMover.Instance.movingEnabled = false;
    }

    public void TriggerMainMenuEventLater()
    {
        SingletonBehaviour<CoroutineManager>.Instance.StartCoroutine(TriggerMainMenuEvent());
    }

    private IEnumerator TriggerMainMenuEvent()
    {
        yield return null;
        while (mainMenuLoadedQueue.Count > 0)
            mainMenuLoadedQueue.Dequeue().Invoke();
    }

    public void QueueMainMenuEvent(Action action)
    {
        mainMenuLoadedQueue.Enqueue(action);
    }

    public void StartServer(int port)
    {
        if (Server != null)
            throw new InvalidOperationException("NetworkManager already exists!");
        Multiplayer.Log($"Starting server on port {port}");
        NetworkServer server = new(Multiplayer.Settings);
        server.Start(port);
        Server = server;
        StartClient("localhost", port, Multiplayer.Settings.Password);
    }

    public void StartClient(string address, int port, string password)
    {
        if (Client != null)
            throw new InvalidOperationException("NetworkManager already exists!");
        NetworkClient client = new(Multiplayer.Settings);
        client.Start(address, port, password);
        Client = client;
    }

    private IEnumerator PollEvents()
    {
        while (true)
        {
            float startTime = Time.realtimeSinceStartup;

            try
            {
                Server?.PollEvents();
            }
            catch (Exception e)
            {
                Multiplayer.Log($"Exception while polling server events: {e}");
            }

            try
            {
                Client?.PollEvents();
            }
            catch (Exception e)
            {
                Multiplayer.Log($"Exception while polling client events: {e}");
            }

            float elapsedTime = Time.realtimeSinceStartup - startTime;
            float remainingTime = Mathf.Max(0f, TICK_INTERVAL - elapsedTime);
            yield return new WaitForSecondsRealtime(remainingTime);
        }
    }

    public void Stop()
    {
        Server?.Stop();
        Client?.Stop();
        Server = null;
        Client = null;
    }

    private void OnApplicationQuit()
    {
        Stop();
    }

    public static void CreateLifecycle()
    {
        if (FindObjectOfType<NetworkLifecycle>() != null)
            throw new InvalidOperationException($"{nameof(NetworkLifecycle)} already exists!");
        GameObject gameObject = new($"[{nameof(NetworkLifecycle)}]");
        gameObject.AddComponent<NetworkLifecycle>();
        DontDestroyOnLoad(gameObject);
    }
}
