namespace Multiplayer.Networking.Packets.Serverbound;

public class CommonChangeJunctionPacket
{
    public ushort Index { get; set; }
    public byte SelectedBranch { get; set; }
    public byte Mode { get; set; }
}
