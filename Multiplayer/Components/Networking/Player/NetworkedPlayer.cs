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
    private Vector3 trueTargetPos;
    private Quaternion targetRotation;
    private Vector2 moveDir;
    private Vector2 targetMoveDir;

    private void Awake()
    {
        animationHandler = GetComponent<AnimationHandler>();

        nameTag = GetComponent<NameTag>();
        nameTag.LookTarget = PlayerManager.PlayerCamera.transform;

        OnSettingsUpdated(Multiplayer.Settings);
        Settings.OnSettingsUpdated += OnSettingsUpdated;

        selfTransform = transform;
        trueTargetPos = selfTransform.position;
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

        Vector3 position = Vector3.Lerp(selfTransform.position, trueTargetPos - WorldMover.currentMove, t);
        Quaternion rotation = Quaternion.Lerp(selfTransform.rotation, targetRotation, t);

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

    public void UpdatePosition(Vector3 position, Vector2 moveDir, float rotation, bool isJumping)
    {
        trueTargetPos = position;
        targetRotation = Quaternion.Euler(0, rotation, 0);
        targetMoveDir = moveDir;

        animationHandler.SetIsJumping(isJumping);
    }

    public void UpdateCar(ushort netId)
    {
        TrainComponentLookup.Instance.TrainFromNetId(netId, out TrainCar trainCar);
        isOnCar = trainCar != null;
        selfTransform.SetParent(isOnCar ? trainCar.transform : null, true);
    }
}
