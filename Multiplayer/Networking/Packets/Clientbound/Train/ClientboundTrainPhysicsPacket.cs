using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundTrainPhysicsPacket
{
    public ushort NetId { get; set; }
    public uint Tick { get; set; }
    public float Speed { get; set; }
    public BogieMovementData Bogie1 { get; set; }
    public BogieMovementData Bogie2 { get; set; }
}
