using System.Collections;
using Humanizer;
using LiteNetLib;
using UnityEngine;
using UnityModManagerNet;

namespace Multiplayer.Components.Networking;

public class NetworkStatsGui : MonoBehaviour
{
    private bool showStats;
    private NetStatistics clientStats;
    private NetStatistics serverStats;

    private long bytesReceivedPerSecond;
    private long bytesSentPerSecond;
    private long packetsReceivedPerSecond;
    private long packetsSentPerSecond;

    private Coroutine updateCoro;

    public void Show(NetStatistics clientStats, NetStatistics serverStats)
    {
        this.clientStats = clientStats;
        clientStats.Reset();
        this.serverStats = serverStats;
        serverStats?.Reset();
        updateCoro = StartCoroutine(UpdateStats());
        showStats = true;
    }

    public void Hide()
    {
        showStats = false;
        if (updateCoro != null)
            StopCoroutine(updateCoro);
    }

    private IEnumerator UpdateStats()
    {
        while (true)
        {
            bytesReceivedPerSecond = serverStats != null ? serverStats.BytesReceived - clientStats.BytesSent : clientStats.BytesReceived;
            bytesSentPerSecond = serverStats != null ? serverStats.BytesSent - clientStats.BytesReceived : clientStats.BytesReceived;
            packetsReceivedPerSecond = serverStats != null ? serverStats.PacketsReceived - clientStats.PacketsSent : clientStats.PacketsReceived;
            packetsSentPerSecond = serverStats != null ? serverStats.PacketsSent - clientStats.PacketsReceived : clientStats.PacketsReceived;
            serverStats?.Reset();
            clientStats?.Reset();
            yield return new WaitForSecondsRealtime(1);
        }
    }

    private void OnGUI()
    {
        if (!showStats)
            return;

        GUILayout.BeginArea(new Rect(10, 25, 250, 75), GUI.skin.box);

        GUILayout.Label("Network Statistics", UnityModManager.UI.bold);

        GUILayout.Label($"Send: {bytesSentPerSecond.Bytes().ToFullWords()}/s ({packetsSentPerSecond:N0} packets/s)");
        GUILayout.Label($"Receive: {bytesReceivedPerSecond.Bytes().ToFullWords()}/s ({packetsReceivedPerSecond:N0} packets/s)");

        GUILayout.EndArea();
    }
}
