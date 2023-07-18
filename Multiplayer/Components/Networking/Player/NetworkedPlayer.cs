using UnityEngine;

namespace Multiplayer.Components.Networking.Player;

public class NetworkedPlayer : MonoBehaviour
{
    // TODO: Interpolate position
    // TODO: animations
    public void UpdatePosition(Vector3 newPosition, float newRotation)
    {
        Transform t = transform;
        t.position = newPosition;
        Vector3 rot = t.eulerAngles;
        rot.y = newRotation;
        t.eulerAngles = rot;
    }
}
