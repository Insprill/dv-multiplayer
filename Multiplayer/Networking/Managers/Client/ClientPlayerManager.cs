using System.Collections.Generic;
using Multiplayer.Components.Networking.Player;
using UnityEngine;

namespace Multiplayer.Networking.Listeners;

public class ClientPlayerManager
{
    private readonly Dictionary<byte, NetworkedPlayer> playerMap = new();

    private readonly GameObject playerPrefab;

    public ClientPlayerManager()
    {
        playerPrefab = Multiplayer.AssetIndex.playerPrefab;
    }

    public void AddPlayer(byte id, string username)
    {
        GameObject go = Object.Instantiate(playerPrefab, WorldMover.Instance.originShiftParent);
        NetworkedPlayer networkedPlayer = go.AddComponent<NetworkedPlayer>();
        playerMap.Add(id, networkedPlayer);
    }

    public void RemovePlayer(byte id)
    {
        if (!playerMap.TryGetValue(id, out NetworkedPlayer player))
            return;
        Object.Destroy(player.gameObject);
        playerMap.Remove(id);
    }

    public void UpdatePosition(byte id, Vector3 position, float rotationY)
    {
        if (playerMap.TryGetValue(id, out NetworkedPlayer player))
            player.UpdatePosition(position, rotationY);
    }
}
