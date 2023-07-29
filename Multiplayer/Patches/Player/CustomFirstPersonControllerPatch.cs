using HarmonyLib;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Patches.Player;

[HarmonyPatch(typeof(CustomFirstPersonController))]
public static class MovementSyncPatch
{
    private static CustomFirstPersonController fps;
    private static Transform fpsTransform;

    private static Vector3 lastPosition;
    private static float lastRotationY;
    private static bool sentFinalPosition;

    private static bool isJumping;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.Awake))]
    private static void CharacterMovement(CustomFirstPersonController __instance)
    {
        fps = __instance;
        fpsTransform = fps.transform;
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
        bool isOnCar = PlayerManager.Car != null;
        Vector3 position = isOnCar ? fpsTransform.localPosition : fpsTransform.position + WorldMover.currentMove;
        float rotationY = (isOnCar ? fpsTransform.localEulerAngles : fpsTransform.eulerAngles).y;

        bool positionOrRotationChanged = lastPosition != position || !Mathf.Approximately(lastRotationY, rotationY);
        if (!positionOrRotationChanged && sentFinalPosition)
            return;

        lastPosition = position;
        lastRotationY = rotationY;
        sentFinalPosition = !positionOrRotationChanged;
        NetworkLifecycle.Instance.Client.SendPlayerPosition(lastPosition, fpsTransform.InverseTransformDirection(fps.m_MoveDir), lastRotationY, isJumping, isJumping || sentFinalPosition);
        isJumping = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CustomFirstPersonController.SetJumpParameters))]
    private static void SetJumpParameters()
    {
        isJumping = true;
    }
}
