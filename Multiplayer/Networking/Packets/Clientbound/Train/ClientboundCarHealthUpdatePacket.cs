namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundCarHealthUpdatePacket
{
    public ushort NetId { get; set; }
    public float Health { get; set; }
}
