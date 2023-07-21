namespace Multiplayer.Networking.Packets.Serverbound;

public class CommonChangeJunctionPacket
{
    public ushort junctionId { get; set; }
    public byte selectedBranch { get; set; }
}
