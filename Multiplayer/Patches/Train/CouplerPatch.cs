using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(Coupler), nameof(Coupler.ConnectAirHose))]
public static class Coupler_ConnectAirHose_Patch
{
    private static void Postfix(Coupler __instance, Coupler other, bool playAudio)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        NetworkLifecycle.Instance.Client?.SendHoseConnected(__instance, other, playAudio);
    }
}

[HarmonyPatch(typeof(Coupler), nameof(Coupler.DisconnectAirHose))]
public static class Coupler_DisconnectAirHose_Patch
{
    private static void Postfix(Coupler __instance, bool playAudio)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        NetworkLifecycle.Instance.Client?.SendHoseDisconnected(__instance, playAudio);
    }
}
