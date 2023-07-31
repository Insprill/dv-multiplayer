using DV.ThingTypes.TransitionHelpers;
using HarmonyLib;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(CargoModelController), nameof(CargoModelController.OnCargoLoaded))]
public static class CargoModelControllerPatch
{
    private static bool Prefix(CargoModelController __instance)
    {
        if (AudioManager.Instance.cargoLoadUnload != null && __instance.trainCar.IsCargoLoadedUnloadedByMachine)
        {
            Transform transform = __instance.trainCar.transform;
            AudioManager.Instance.cargoLoadUnload.Play(transform.position, minDistance: 10f, parent: transform);
        }

        GameObject[] prefabsForCarType = __instance.trainCar.LoadedCargo.ToV2().GetCargoPrefabsForCarType(__instance.trainCar.carLivery.parentType);
        if (prefabsForCarType == null || prefabsForCarType.Length == 0)
            return false;

        NetworkedTrainCar networkedTrainCar = __instance.trainCar.GetNetworkedCar();

        byte modelIndex = networkedTrainCar.CargoModelIndex;
        if (modelIndex == byte.MaxValue || modelIndex >= prefabsForCarType.Length)
        {
            modelIndex = (byte)Random.Range(0, prefabsForCarType.Length);
            networkedTrainCar.CargoModelIndex = modelIndex;
        }

        __instance.currentCargoModel = Object.Instantiate(prefabsForCarType[modelIndex], __instance.trainCar.interior.transform, false);
        __instance.currentCargoModel.transform.localPosition = Vector3.zero;
        __instance.currentCargoModel.transform.localRotation = Quaternion.identity;
        __instance.trainColliders.SetupCargo(__instance.currentCargoModel);
        return false;
    }
}
