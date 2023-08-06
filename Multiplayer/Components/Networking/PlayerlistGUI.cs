using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Multiplayer.Networking.Listeners;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Components.Networking;

public class PlayerlistGUI : MonoBehaviour
{
    [CanBeNull]
    private NetworkClient _client;
    private bool showPlayerlist;

    public void Show([CanBeNull] NetworkClient client)
    {
        _client = client;
        showPlayerlist = true;
    }

    public void Hide()
    {
        showPlayerlist = false;
    }

    private void OnGUI()
    {
        if (!showPlayerlist)
            return;

        GUILayout.Window(157031520, new Rect((Screen.width/2)-125, 25, 250, 0), DrawPlayerlist, "Online Players");
    }

    private void DrawPlayerlist(int windowId)
    {
        foreach (string player in GetPlayerlist())
        {
            GUILayout.Label(player);
        }
    }

    private IEnumerable<string> GetPlayerlist()
    {
        if (_client is not { IsRunning: true }) return new[] { "Not in game" };
        List<string> playerlist = _client.PlayerManager.Players.Select(x => $"{x.Username} ({x.GetPing().ToString()}ms)").ToList();
        // The Player of the Client is not in the PlayerManager, so we need to add it seperatly
        playerlist.Add($"{Multiplayer.Settings.Username} ({_client.Ping.ToString()}ms)");
        return playerlist;
    }
}
