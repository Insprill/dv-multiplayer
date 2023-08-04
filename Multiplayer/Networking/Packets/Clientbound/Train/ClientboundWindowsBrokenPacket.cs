using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundWindowsBrokenPacket
{
    public ushort NetId { get; set; }
    public Vector3 ForceDirection { get; set; }
}
