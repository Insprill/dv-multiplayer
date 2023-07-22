namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonCockFiddlePacket
{
    public string CarGUID { get; set; }
    public bool IsFront { get; set; }
    public bool IsOpen { get; set; }
}
