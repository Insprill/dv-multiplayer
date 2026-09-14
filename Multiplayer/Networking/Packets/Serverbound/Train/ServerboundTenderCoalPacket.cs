namespace Multiplayer.Networking.Packets.Serverbound.Train;

public class ServerboundTenderCoalPacket
{
    public ushort NetId { get; set; }
    public float CoalMassDelta { get; set; }
}
