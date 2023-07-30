using Multiplayer.Editor.Components.Player;
using UnityEngine;

namespace Multiplayer.Components.Networking.Player;

public class NetworkedPlayer : MonoBehaviour
{
    private const float LERP_SPEED = 5.0f;

    private AnimationHandler animationHandler;
    private NameTag nameTag;

    private string username;
    private bool isOnCar;

    private Transform selfTransform;
    private Vector3 targetPos;
    private Quaternion targetRotation;
    private Vector2 moveDir;
    private Vector2 targetMoveDir;

    private void Awake()
    {
        animationHandler = GetComponent<AnimationHandler>();

        nameTag = GetComponent<NameTag>();
        nameTag.LookTarget = PlayerManager.ActiveCamera.transform;
        PlayerManager.CameraChanged += () => nameTag.LookTarget = PlayerManager.ActiveCamera.transform;

        OnSettingsUpdated(Multiplayer.Settings);
        Settings.OnSettingsUpdated += OnSettingsUpdated;

        selfTransform = transform;
        targetPos = selfTransform.position;
        targetRotation = selfTransform.rotation;
        moveDir = Vector2.zero;
        targetMoveDir = Vector2.zero;
    }

    private void OnSettingsUpdated(Settings settings)
    {
        nameTag.ShowUsername(settings.ShowNameTags);
        nameTag.ShowPing(settings.ShowNameTags && settings.ShowPingInNameTags);
    }

    public void SetUsername(string newUsername)
    {
        username = newUsername;
        nameTag.SetUsername(username);
    }

    public void SetPing(int ping)
    {
        nameTag.SetPing(ping);
    }

    private void Update()
    {
        float t = Time.deltaTime * LERP_SPEED;

        Vector3 position = Vector3.Lerp(isOnCar ? selfTransform.localPosition : selfTransform.position, isOnCar ? targetPos : targetPos - WorldMover.currentMove, t);
        Quaternion rotation = Quaternion.Lerp(isOnCar ? selfTransform.localRotation : selfTransform.rotation, targetRotation, t);

        moveDir = Vector2.Lerp(moveDir, targetMoveDir, t);
        animationHandler.SetMoveDir(moveDir);

        if (isOnCar)
        {
            selfTransform.localPosition = position;
            selfTransform.localRotation = rotation;
        }
        else
        {
            selfTransform.position = position;
            selfTransform.rotation = rotation;
        }
    }

    public void UpdatePosition(Vector3 position, Vector2 moveDir, float rotation, bool isJumping, bool movePacketIsOnCar)
    {
        targetMoveDir = moveDir;
        animationHandler.SetIsJumping(isJumping);

        if (isOnCar != movePacketIsOnCar)
            return;

        targetPos = position;
        targetRotation = Quaternion.Euler(0, rotation, 0);
    }

    public void UpdateCar(ushort netId)
    {
        TrainComponentLookup.Instance.TrainFromNetId(netId, out TrainCar trainCar);
        isOnCar = trainCar != null;
        selfTransform.SetParent(isOnCar ? trainCar.transform : null, true);
        targetPos = isOnCar ? transform.localPosition : selfTransform.position;
        targetRotation = isOnCar ? transform.localRotation : selfTransform.rotation;
    }
}
