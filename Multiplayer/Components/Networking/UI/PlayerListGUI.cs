using System.Collections.Generic;
using Multiplayer.Components.Networking.Player;
using UnityEngine;

namespace Multiplayer.Components.Networking.UI;

public class PlayerListGUI : MonoBehaviour
{
    private const float WINDOW_WIDTH = 250;
    private const float WINDOW_HEIGHT = 0; // Height will be determined by the content
    private const float WINDOW_PADDING = 25;

    public enum PlayerListPosition
    {
        TopLeft,
        TopRight,
        TopCenter
    }

    private bool showPlayerList;
    private string LocalPlayerUsername => NetworkLifecycle.Instance?.Client?.DisplayName ?? Multiplayer.Settings.GetUserName();

    public void RegisterListeners()
    {
        ScreenspaceMouse.Instance.ValueChanged += OnToggle;
    }

    public void UnRegisterListeners()
    {
        ScreenspaceMouse.Instance.ValueChanged -= OnToggle;
        OnToggle(false);
    }

    private void OnToggle(bool status)
    {
        showPlayerList = status && Multiplayer.Settings.ShowPlayerListInAltMouseMode;
    }

    protected void OnGUI()
    {
        if (!showPlayerList)
            return;

        var position = Multiplayer.Settings.PlayerListPosition;
        Rect windowPos;

        if (position == PlayerListPosition.TopCenter || (position == PlayerListPosition.TopLeft && Multiplayer.Settings.ShowStats))
        {
            windowPos = new Rect((Screen.width - WINDOW_WIDTH)/2f, WINDOW_PADDING, WINDOW_WIDTH, WINDOW_HEIGHT);
        }
        else if (position == PlayerListPosition.TopRight)
        {
            windowPos = new Rect(Screen.width - WINDOW_WIDTH - WINDOW_PADDING, WINDOW_PADDING, WINDOW_WIDTH, WINDOW_HEIGHT);
        }
        else
        {
            windowPos = new Rect(WINDOW_PADDING, WINDOW_PADDING, WINDOW_WIDTH, WINDOW_HEIGHT);
        }

        GUILayout.Window(157031520, windowPos, DrawPlayerList, Locale.PLAYER_LIST__TITLE);
    }

    private void DrawPlayerList(int windowId)
    {
        foreach (string player in GetPlayerList())
            GUILayout.Label(player);
    }

    // todo: cache this?
    private IEnumerable<string> GetPlayerList()
    {
        if (!NetworkLifecycle.Instance.IsClientRunning)
            return new[] { "Not in game" };

        IReadOnlyCollection<NetworkedPlayer> players = NetworkLifecycle.Instance.Client.ClientPlayerManager.Players;
        string[] playerList = new string[players.Count + 1];
        int i = 0;
        foreach (NetworkedPlayer player in players)
        {
            playerList[i] = $"{player.DisplayName} ({player.GetPing().ToString()}ms)";
            i++;
        }

        // The Player of the Client is not in the PlayerManager, so we need to add it separately
        playerList[playerList.Length - 1] = $"{LocalPlayerUsername} ({NetworkLifecycle.Instance.Client.Ping}ms)";
        return playerList;
    }
}
