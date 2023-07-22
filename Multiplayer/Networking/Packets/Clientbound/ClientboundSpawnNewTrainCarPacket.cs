using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundSpawnNewTrainCarPacket
{
    public string Id { get; set; }
    public string Track { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }
    public bool PlayerSpawnedCar { get; set; }
}
