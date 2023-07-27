using DV.MultipleUnit;
using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(MultipleUnitCable), nameof(MultipleUnitCable.Connect))]
public static class MultipleUnitCable_Connect_Patch
{
    private static void Postfix(MultipleUnitCable __instance, MultipleUnitCable other, bool playAudio)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket || UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Client?.SendMuConnected(__instance, other, playAudio);
    }
}

[HarmonyPatch(typeof(MultipleUnitCable), nameof(MultipleUnitCable.Disconnect))]
public static class MultipleUnitCable_Disconnect_Patch
{
    private static void Postfix(MultipleUnitCable __instance, bool playAudio)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket || UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Client?.SendMuDisconnected(__instance, playAudio);
    }
}
