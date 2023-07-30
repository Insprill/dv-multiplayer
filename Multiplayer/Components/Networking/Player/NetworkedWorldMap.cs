using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Multiplayer.Components.Networking.Player;

public class NetworkedWorldMap : MonoBehaviour
{
    private WorldMap worldMap;
    private MapMarkersController markersController;
    private GameObject textPrefab;
    private readonly Dictionary<byte, Transform> playerIndicators = new();

    private void Awake()
    {
        worldMap = GetComponent<WorldMap>();
        markersController = GetComponent<MapMarkersController>();
        textPrefab = worldMap.GetComponentInChildren<TMP_Text>().gameObject;
        foreach (NetworkedPlayer networkedPlayer in NetworkLifecycle.Instance.Client.PlayerManager.Players)
            OnPlayerConnected(networkedPlayer.Id, networkedPlayer);
        NetworkLifecycle.Instance.Client.PlayerManager.OnPlayerConnected += OnPlayerConnected;
        NetworkLifecycle.Instance.Client.PlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;
        NetworkLifecycle.Instance.OnTick += OnTick;
    }

    private void OnDestroy()
    {
        if (UnloadWatcher.isQuitting)
            return;
        NetworkLifecycle.Instance.OnTick -= OnTick;
        if (UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Client.PlayerManager.OnPlayerConnected -= OnPlayerConnected;
        NetworkLifecycle.Instance.Client.PlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
    }

    private void OnPlayerConnected(byte id, NetworkedPlayer player)
    {
        Transform indicator = new GameObject($"{player}'s Indicator") {
            transform = {
                parent = worldMap.playerIndicator.parent
            }
        }.transform;
        Multiplayer.LogDebug(() => $"Creating indicator for {player.Username}. Parent: {worldMap.playerIndicator.parent} Markers Displayed: {worldMap.gameParams.PlayerMarkerDisplayed}");
        Instantiate(worldMap.playerIndicator.gameObject, indicator);
        TMP_Text text = Instantiate(textPrefab, indicator).GetComponent<TMP_Text>();
        text.text = player.Username;
        text.fontSize /= 1.5f;
        playerIndicators[id] = indicator;
    }

    private void OnPlayerDisconnected(byte id, NetworkedPlayer player)
    {
        if (!playerIndicators.TryGetValue(id, out Transform indicator))
            return;
        Destroy(indicator.gameObject);
        playerIndicators.Remove(id);
    }

    private void OnTick(uint obj)
    {
        if (!worldMap.initialized)
            return;
        UpdatePlayers();
    }

    public void UpdatePlayers()
    {
        foreach (KeyValuePair<byte, Transform> kvp in playerIndicators)
        {
            if (!NetworkLifecycle.Instance.Client.PlayerManager.TryGetPlayer(kvp.Key, out NetworkedPlayer networkedPlayer))
            {
                Multiplayer.LogWarning($"Player indicator for {kvp.Key} exists but {nameof(NetworkedPlayer)} does not!");
                OnPlayerDisconnected(kvp.Key, null);
                continue;
            }

            Transform indicatorTransform = kvp.Value;
            Transform playerTransform = networkedPlayer.transform;

            bool active = worldMap.gameParams.PlayerMarkerDisplayed;
            if (indicatorTransform.gameObject.activeSelf != active)
                indicatorTransform.gameObject.SetActive(active);

            Vector3 normalized = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized;
            if (normalized != Vector3.zero)
                indicatorTransform.localRotation = Quaternion.LookRotation(normalized);

            indicatorTransform.localPosition = markersController.GetMapPosition(playerTransform.position, worldMap.triggerExtentsXZ);
        }
    }
}
