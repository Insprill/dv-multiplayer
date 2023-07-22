using HarmonyLib;
using Multiplayer.Components.Networking;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CarsSaveManager), nameof(CarsSaveManager.Load))]
public class CarsSaveManager_Load_Patch
{
    private static bool Prefix()
    {
        if (!NetworkLifecycle.Instance.IsClientRunning || NetworkLifecycle.Instance.IsHost())
            return true;
        CarsSaveManager.DeleteAllExistingCars();
        return false;
    }
}
