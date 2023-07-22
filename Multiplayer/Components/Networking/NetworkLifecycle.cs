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
    public NetworkServer Server { get; private set; }
    public NetworkClient Client { get; private set; }

    public bool IsServerRunning => Server?.IsRunning ?? false;
    public bool IsClientRunning => Client?.IsRunning ?? false;

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

    private void FixedUpdate()
    {
        Server?.PollEvents();
        Client?.PollEvents();
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
