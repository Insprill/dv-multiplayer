using DV;
using DV.InventorySystem;
using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.CommsRadio;

[HarmonyPatch(typeof(CommsRadioCarDeleter))]
public static class CommsRadioCarDeleterPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CommsRadioCarDeleter.OnUse))]
    private static bool OnUse_Prefix(CommsRadioCarDeleter __instance)
    {
        if (NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1)
            return true;
        if (__instance.state != CommsRadioCarDeleter.State.ConfirmDelete)
            return true;
        if (__instance.carToDelete.GetNetworkedCar().HasPlayers)
            return false;
        if (Inventory.Instance.PlayerMoney <= __instance.removePrice)
            return true;
        NetworkLifecycle.Instance.Client.SendTrainDeleteRequest(__instance.carToDelete.GetNetId());
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CommsRadioCarDeleter.OnUpdate))]
    private static bool OnUpdate_Prefix(CommsRadioCarDeleter __instance)
    {
        if (NetworkLifecycle.Instance.IsHost() && NetworkLifecycle.Instance.Server.PlayerCount == 1)
            return true;
        if (__instance.state != CommsRadioCarDeleter.State.ScanCarToDelete)
            return true;
        if (!Physics.Raycast(__instance.signalOrigin.position, __instance.signalOrigin.forward, out __instance.hit, CommsRadioCarDeleter.SIGNAL_RANGE, __instance.trainCarMask))
            return true;
        TrainCar car = TrainCar.Resolve(__instance.hit.transform.root);
        if (car != null && !car.GetNetworkedCar().HasPlayers)
            return true;
        __instance.PointToCar(null);
        return false;
    }
}
