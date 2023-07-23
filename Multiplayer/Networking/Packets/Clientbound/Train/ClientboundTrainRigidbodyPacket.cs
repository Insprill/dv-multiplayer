using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundTrainRigidbodyPacket
{
    public ushort NetId { get; set; }
    public RigidBodyData Car { get; set; }
    public RigidBodyData Bogie1 { get; set; }
    public RigidBodyData Bogie2 { get; set; }
}
