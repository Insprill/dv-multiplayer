using HarmonyLib;
using Multiplayer.Components;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Utils;

namespace Multiplayer.Patches.World;

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Awake))]
public class TrainCar_Awake_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        __instance.gameObject.GetOrAddComponent<NetworkedTrainCar>();
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.SetupRigidbody))]
public class TrainCar_SetupRigidbody_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        if (!NetworkLifecycle.Instance.IsHost())
            __instance.gameObject.GetOrAddComponent<TrainSpeedQueue>();
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.Start))]
public class TrainCar_Start_Patch
{
    private static void Prefix(TrainCar __instance)
    {
        TrainComponentLookup.Instance.RegisterTrainCar(__instance);
    }
}

[HarmonyPatch(typeof(TrainCar), nameof(TrainCar.PrepareForDestroy))]
public class TrainCar_PrepareForDestroy_Patch
{
    private static void Postfix(TrainCar __instance)
    {
        NetworkLifecycle.Instance.Server?.SendDestroyTrainCar(__instance);
        TrainComponentLookup.Instance.UnregisterTrainCar(__instance);
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
        TrainComponentLookup.Instance.UnregisterTrainCar(__instance);
    }
}
