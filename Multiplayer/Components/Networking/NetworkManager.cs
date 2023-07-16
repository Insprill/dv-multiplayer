using System;
using DV.Utils;
using LiteNetLib;
using UnityEngine;

namespace Multiplayer.Components.Networking;

// https://revenantx.github.io/LiteNetLib/index.html
public abstract class NetworkManager : SingletonBehaviour<NetworkManager>
{
    private EventBasedNetListener listener;
    private NetManager netManager;

    protected override void Awake()
    {
        base.Awake();
        listener = new EventBasedNetListener();
        netManager = new NetManager(listener);
        OnSettingsUpdated(Multiplayer.Settings);
        Multiplayer.Settings.OnSettingsUpdated += OnSettingsUpdated;
    }

    private void OnSettingsUpdated(Settings settings)
    {
        netManager.NatPunchEnabled = settings.EnableNatPunch;
        netManager.AutoRecycle = settings.ReuseNetPacketReaders;
        netManager.UseNativeSockets = settings.UseNativeSockets;
    }

    #region Common

    public void Stop()
    {
        netManager?.Stop(true);
    }

    private void FixedUpdate()
    {
        netManager?.PollEvents();
    }

    #endregion

    #region Server

    public void StartServer(int port)
    {
        netManager.Start(port);
        listener.ConnectionRequestEvent += OnConnectionRequest;
        listener.PeerConnectedEvent += OnPeerConnected;
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (netManager.ConnectedPeersCount < Multiplayer.Settings.MaxPlayers)
            request.AcceptIfKey(Multiplayer.Settings.Password);
        else
            request.Reject();
    }

    private void OnPeerConnected(NetPeer peer)
    {
        // todo: send ack, client can then start loading
    }

    #endregion

    #region Client

    public void StartClient(string address, ushort port, string password)
    {
        netManager.Start();
        netManager.Connect(address, port, password);
        listener.NetworkReceiveEvent += OnNetworkReceive;
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    { }

    #endregion

    public static void CreateNetworkManager()
    {
        if (FindObjectOfType<NetworkManager>() != null)
            throw new InvalidOperationException($"{nameof(NetworkManager)} already exists!");
        GameObject gameObject = new($"[{nameof(NetworkManager)}]");
        gameObject.AddComponent<NetworkManager>();
        DontDestroyOnLoad(gameObject);
    }
}
