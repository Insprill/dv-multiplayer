using UnityEngine;

namespace Multiplayer.Networking.Data;

public class ServerPlayer
{
    public byte Id { get; set; }
    public string Username { get; set; }
    public Vector3 RawPosition { get; set; }
    public float RawRotationY { get; set; }
    public ushort CarId { get; set; }

    public override string ToString()
    {
        return $"{Id} ({Username})";
    }
}
