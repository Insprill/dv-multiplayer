using Multiplayer.Editor.Components.Player;
using UnityEngine;

namespace Multiplayer.Components.Networking.Player;

public class NetworkedPlayer : MonoBehaviour
{
    public string username;
    private AnimationHandler animationHandler;
    private bool isOnCar;

    private void Awake()
    {
        animationHandler = GetComponent<AnimationHandler>();
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
