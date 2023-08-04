using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundRerailTrainPacket
{
    public ushort NetId { get; set; }
    public ushort TrackId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }
}
