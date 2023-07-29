using Multiplayer.Editor.Components.Player;
using UnityEngine;

namespace Multiplayer.Components.Networking.Player;

public class NetworkedPlayer : MonoBehaviour
{
    private string username;
    private PlayerComponents playerComponents;
    private AnimationHandler animationHandler;
    private bool isOnCar;

    private void Awake()
    {
        playerComponents = GetComponent<PlayerComponents>();
        animationHandler = GetComponent<AnimationHandler>();
        playerComponents.nameTag.LookTarget = PlayerManager.PlayerCamera.transform;
        OnSettingsUpdated(Multiplayer.Settings);
        Settings.OnSettingsUpdated += OnSettingsUpdated;
    }

    private void OnSettingsUpdated(Settings settings)
    {
        playerComponents.nameTag.ShowUsername(settings.ShowNameTags);
        playerComponents.nameTag.ShowPing(settings.ShowNameTags && settings.ShowPingInNameTags);
    }

    public void SetUsername(string newUsername)
    {
        username = newUsername;
        playerComponents.nameTag.SetUsername(username);
    }

    public void SetPing(int ping)
    {
        playerComponents.nameTag.SetPing(ping);
    }

    public void UpdatePosition(Vector3 position, float rotation, bool isJumping)
    {
        if (isOnCar)
        {
            transform.localPosition = position;
            transform.localRotation = Quaternion.Euler(0, rotation, 0);
        }
        else
        {
            transform.position = position - WorldMover.currentMove;
            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        // animationHandler.SetSpeed(?);

        if (isJumping)
            animationHandler.Jump();
    }

    public void UpdateCar(ushort netId)
    {
        TrainComponentLookup.Instance.TrainFromNetId(netId, out TrainCar trainCar);
        isOnCar = trainCar != null;
        transform.SetParent(isOnCar ? trainCar.transform : null, true);
    }
}
