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
        GUILayout.BeginArea(new Rect((Screen.width/2)-50, 10 , 100, 100));
        GUILayout.BeginVertical(GUI.skin.box);

        GUILayout.Label("Players", UnityModManager.UI.bold);
        foreach (string player in currentPlayerlist)
        {
            GUILayout.Label(player);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea ();
    }
}
