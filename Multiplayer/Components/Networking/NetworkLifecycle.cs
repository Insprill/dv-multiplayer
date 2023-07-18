using System;
using System.Net;
using DV.Utils;
using Multiplayer.Networking.Listeners;
using UnityEngine;

namespace Multiplayer.Components.Networking;

// https://revenantx.github.io/LiteNetLib/index.html
public class NetworkLifecycle : SingletonBehaviour<NetworkLifecycle>
{
    public NetworkServer Server { get; private set; }
    public NetworkClient Client { get; private set; }

    public void StartServer(int port)
    {
        if (Server != null)
            throw new InvalidOperationException("NetworkManager already exists!");
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

    public static void CreateLifecycle()
    {
        if (FindObjectOfType<NetworkLifecycle>() != null)
            throw new InvalidOperationException($"{nameof(NetworkLifecycle)} already exists!");
        GameObject gameObject = new($"[{nameof(NetworkLifecycle)}]");
        gameObject.AddComponent<NetworkLifecycle>();
        DontDestroyOnLoad(gameObject);
    }
}
