namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonCockFiddlePacket
{
    public ushort NetId { get; set; }
    public bool IsFront { get; set; }
    public bool IsOpen { get; set; }
}
