using System;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Components.Networking;

public class PlayerlistGUI : MonoBehaviour
{
    private IEnumerable<string> currentPlayerlist = Array.Empty<string>();
    private bool showPlayerlist;

    public void Show(IEnumerable<string> newPlayerlist)
    {
        currentPlayerlist = newPlayerlist;
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
        foreach (string player in currentPlayerlist)
        {
            GUILayout.Label(player);
        }
    }
}