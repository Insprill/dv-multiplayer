namespace Multiplayer.Networking.Packets.Serverbound;

public class CommonChangeJunctionPacket
{
    public ushort index { get; set; }
    public byte selectedBranch { get; set; }
}
