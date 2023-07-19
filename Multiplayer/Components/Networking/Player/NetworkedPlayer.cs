using Multiplayer.Editor.Components.Player;
using UnityEngine;

namespace Multiplayer.Components.Networking.Player;

public class NetworkedPlayer : MonoBehaviour
{
    public string username;
    private AnimationHandler animationHandler;

    private void Awake()
    {
        animationHandler = GetComponent<AnimationHandler>();
    }

    public void UpdatePosition(Vector3 position, float rotation, bool isJumping)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, rotation, 0);

        // animationHandler.SetSpeed(?);

        if (isJumping)
            animationHandler.Jump();
    }
}
