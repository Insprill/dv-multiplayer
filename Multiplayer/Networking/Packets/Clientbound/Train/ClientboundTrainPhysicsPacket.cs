using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundTrainPhysicsPacket
{
    public ushort NetId { get; set; }
    public int Timestamp { get; set; }
    public RigidbodySnapshot Car { get; set; }
    public RigidbodySnapshot Bogie1 { get; set; }
    public RigidbodySnapshot Bogie2 { get; set; }
}
