using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(Junction), nameof(Junction.Switch))]
public static class Junction_Switched_Patch
{
    private static void Postfix(Junction __instance, Junction.SwitchMode mode)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;

        ushort index = WorldComponentLookup.Instance.IndexFromJunction(__instance);
        NetworkLifecycle.Instance.Client.SendJunctionSwitched(index, (byte)__instance.selectedBranch, mode);
    }
}
