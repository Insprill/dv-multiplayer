using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.Player;

[HarmonyPatch(typeof(CustomFirstPersonController))]
public static class MovementSyncPatch
{
    private static CustomFirstPersonController fps;

    private static Vector3 lastPosition;
    private static float lastRotationY;
    private static bool sentFinalPosition;

    private static bool isJumping;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.Awake))]
    private static void CharacterMovement(CustomFirstPersonController __instance)
    {
        fps = __instance;
        NetworkLifecycle.Instance.OnTick += OnTick;
        PlayerManager.CarChanged += OnCarChanged;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.OnDestroy))]
    private static void OnDestroy()
    {
        if (UnloadWatcher.isQuitting)
            return;
        NetworkLifecycle.Instance.OnTick -= OnTick;
        PlayerManager.CarChanged -= OnCarChanged;
    }

    private static void OnCarChanged(TrainCar trainCar)
    {
        NetworkLifecycle.Instance.Client.SendPlayerCar(trainCar == null ? ushort.MaxValue : trainCar.GetNetId());
    }

    private static void OnTick(uint obj)
    {
        Transform t = fps.transform;
        bool isOnCar = PlayerManager.Car != null;
        Vector3 position = isOnCar ? t.localPosition : t.position + WorldMover.currentMove;
        float rotationY = (isOnCar ? t.localEulerAngles : t.eulerAngles).y;

        bool positionOrRotationChanged = lastPosition != position || !Mathf.Approximately(lastRotationY, rotationY);
        if (!positionOrRotationChanged && sentFinalPosition)
            return;

        lastPosition = position;
        lastRotationY = rotationY;
        sentFinalPosition = !positionOrRotationChanged;
        NetworkLifecycle.Instance.Client.SendPlayerPosition(lastPosition, lastRotationY, isJumping, sentFinalPosition);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.SetJumpParameters))]
    private static void SetJumpParameters()
    {
        isJumping = true;
    }
}
