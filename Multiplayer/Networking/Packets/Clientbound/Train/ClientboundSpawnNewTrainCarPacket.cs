using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundSpawnNewTrainCarPacket
{
    public ushort NetId { get; set; }
    public string LiveryId { get; set; }
    public string Track { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }
    public bool PlayerSpawnedCar { get; set; }
}
