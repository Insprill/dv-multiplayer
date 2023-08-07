using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DV.Scenarios.Common;
using DV.Utils;
using LiteNetLib;
using LiteNetLib.Utils;
using Multiplayer.Components.Networking.Player;
using Multiplayer.Networking.Listeners;
using Multiplayer.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer.Components.Networking;

// https://revenantx.github.io/LiteNetLib/index.html
public class NetworkLifecycle : SingletonBehaviour<NetworkLifecycle>
{
    public const byte TICK_RATE = 24;
    private const float TICK_INTERVAL = 1.0f / TICK_RATE;

    public NetworkServer Server { get; private set; }
    public NetworkClient Client { get; private set; }

    public uint Tick { get; internal set; }
    public Action<uint> OnTick;

    public bool IsServerRunning => Server?.IsRunning ?? false;
    public bool IsClientRunning => Client?.IsRunning ?? false;

    public bool IsProcessingPacket => Client.IsProcessingPacket;

    private NetworkStatsGui Stats;
    private PlayerlistGUI Playerlist;
    private readonly ExecutionTimer tickTimer = new();
    private readonly ExecutionTimer tickWatchdog = new(0.25f);

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
        Playerlist = gameObject.AddComponent<PlayerlistGUI>();
        Stats = gameObject.AddComponent<NetworkStatsGui>();
        RegisterPackets();
        Settings.OnSettingsUpdated += OnSettingsUpdated;
        Settings.OnPlayerlistVisibilityUpdate += OnPlayerlistVisibilityUpdate;
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.buildIndex != (int)DVScenes.MainMenu)
                return;
            TriggerMainMenuEventLater();
        };
        StartCoroutine(PollEvents());
    }

    private static void RegisterPackets()
    {
        IReadOnlyDictionary<Type, byte> packetMappings = NetPacketProcessor.RegisterPacketTypes();
        Multiplayer.LogDebug(() =>
        {
            StringBuilder stringBuilder = new($"Registered {packetMappings.Count} packets. Mappings:\n");
            foreach (KeyValuePair<Type, byte> kvp in packetMappings)
                stringBuilder.AppendLine($"{kvp.Value}: {kvp.Key}");
            return stringBuilder;
        });
    }

    private void OnSettingsUpdated(Settings settings)
    {
        if (!IsClientRunning && !IsServerRunning)
            return;
        if (settings.ShowStats)
            Stats.Show(Client.Statistics, Server?.Statistics);
        else
            Stats.Hide();
    }

    private void OnPlayerlistVisibilityUpdate(bool show)
    {
        if (show)
        {
            Playerlist.Show();
        }
        else
            Playerlist.Hide();
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

    public bool StartServer(int port, IDifficulty difficulty)
    {
        if (Server != null)
            throw new InvalidOperationException("NetworkManager already exists!");
        Multiplayer.Log($"Starting server on port {port}");
        NetworkServer server = new(difficulty, Multiplayer.Settings);
        if (!server.Start(port))
            return false;
        Server = server;
        StartClient("localhost", port, Multiplayer.Settings.Password);
        return true;
    }

    public void StartClient(string address, int port, string password)
    {
        if (Client != null)
            throw new InvalidOperationException("NetworkManager already exists!");
        NetworkClient client = new(Multiplayer.Settings);
        client.Start(address, port, password);
        Client = client;
        OnSettingsUpdated(Multiplayer.Settings); // Show stats if enabled
    }

    private IEnumerator PollEvents()
    {
        while (!UnloadWatcher.isQuitting)
        {
            Tick++;
            tickTimer.Start();

            tickWatchdog.Start();
            try
            {
                if (!UnloadWatcher.isUnloading)
                    OnTick?.Invoke(Tick);
            }
            catch (Exception e)
            {
                Multiplayer.LogError($"Exception while processing OnTick: {e}");
            }
            finally
            {
                tickWatchdog.Stop(time => Multiplayer.LogWarning($"OnTick took {time} ms!"));
            }

            TickManager(Client);
            TickManager(Server);

            float elapsedTime = tickTimer.Stop();
            float remainingTime = Mathf.Max(0f, TICK_INTERVAL - elapsedTime);
            yield return remainingTime < 0.001f ? null : new WaitForSecondsRealtime(remainingTime);
        }
    }

    private void TickManager(NetworkManager manager)
    {
        if (manager == null)
            return;
        tickWatchdog.Start();
        try
        {
            manager.PollEvents();
        }
        catch (Exception e)
        {
            manager.LogError($"Exception while polling events: {e}");
        }
        finally
        {
            tickWatchdog.Stop(time => manager.LogWarning($"PollEvents took {time} ms!"));
        }
    }

    public void Stop()
    {
        if (Stats != null) Stats.Hide();
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
