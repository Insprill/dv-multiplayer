using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Awake))]
public class TrainCar_Awake_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        __instance.LogicCarInitialized += () => TrainComponentLookup.Instance.RegisterTrainCarGUID(__instance);

        if (!NetworkLifecycle.Instance.IsHost())
            return;
        if (__instance.GetComponent<NetworkedTrainCar>() == null)
            __instance.gameObject.AddComponent<NetworkedTrainCar>();
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.PrepareForDestroy))]
public class TrainCar_PrepareForDestroy_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        NetworkLifecycle.Instance.Server?.SendDestroyTrainCar(__instance);
        TrainComponentLookup.Instance.UnregisterTrainCarGUID(__instance);
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.OnDestroy))]
public class TrainCar_OnDestroy_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        if (UnloadWatcher.isUnloading)
            return;
        NetworkLifecycle.Instance.Server?.SendDestroyTrainCar(__instance);
        TrainComponentLookup.Instance.UnregisterTrainCarGUID(__instance);
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.SetupCouplers))]
public class TrainCar_SetupCouplers_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        foreach (Coupler coupler in __instance.couplers)
            TrainComponentLookup.Instance.RegisterHose(coupler.hoseAndCock, coupler);
    }
}
