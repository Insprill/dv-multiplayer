using HarmonyLib;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(WorldMover), nameof(WorldMover.MoveWorld))]
public static class WorldMoverPatch
{
    private static bool Prefix()
    {
        return false;
    }
}
