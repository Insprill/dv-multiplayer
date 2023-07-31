using UnityEngine;

namespace Multiplayer.Networking.Packets.Common;

public class ServerPlayer
{
    public byte Id { get; set; }
    public string Username { get; set; }
    public int Ping { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public TrainCar Car { get; set; }

    public override string ToString()
    {
        return $"{Id} ({Username})";
    }
}
