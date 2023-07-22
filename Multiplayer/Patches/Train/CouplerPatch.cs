using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(Coupler), nameof(Coupler.CoupleTo))]
public static class Coupler_CoupleTo_Patch
{
    private static void Postfix(Coupler __instance, Coupler other, bool playAudio, bool viaChainInteraction)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        NetworkLifecycle.Instance.Client?.SendTrainCouple(__instance, other, playAudio, viaChainInteraction);
    }
}

[HarmonyPatch(typeof(Coupler), nameof(Coupler.Uncouple))]
public static class Coupler_Uncouple_Patch
{
    private static void Postfix(Coupler __instance, bool playAudio, bool calledOnOtherCoupler, bool dueToBrokenCouple, bool viaChainInteraction)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket || calledOnOtherCoupler)
            return;
        NetworkLifecycle.Instance.Client?.SendTrainUncouple(__instance, playAudio, dueToBrokenCouple, viaChainInteraction);
    }
}

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
