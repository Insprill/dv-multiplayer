using DV;
using HarmonyLib;
using Multiplayer.Components.Networking;
using System;
using System.Diagnostics;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TimeAdvance))]
public static class TimeAdvancePatch
{
    public static bool FastTravelAdvancesTime { get; set; } = true;

    [HarmonyPatch(typeof(TimeAdvance), nameof(TimeAdvance.AdvanceTime))]
    private static bool Prefix(float amountOfTimeToSkipInSeconds)
    {
        // Todo: fast travel should be a request to the server, and the server should decide whether to allow it or not and handle time advancement. This is a temporary solution

        // No client - allow time to advance normally
        if (!NetworkLifecycle.Instance.IsClientRunning)
            return true;

        // Host is the only player - allow time to advance normally
        if (NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1)
            return true;

        if (NetworkLifecycle.Instance.IsHost())
        {
            // Host with other players connected - only advance if FastTravelAdvancesTime is enabled
            if (!FastTravelAdvancesTime)
                return false;

            NetworkLifecycle.Instance.Client.SendTimeAdvance(amountOfTimeToSkipInSeconds);
            return true;
        }

        // Client: receiving a relayed packet - allow time to advance, but don't echo back to server
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return true;

        // Client: local fast travel - only allow if FastTravelAdvancesTime is enabled
        if (!FastTravelAdvancesTime)
            return false;

        NetworkLifecycle.Instance.Client.SendTimeAdvance(amountOfTimeToSkipInSeconds);
        return true;
    }
}
