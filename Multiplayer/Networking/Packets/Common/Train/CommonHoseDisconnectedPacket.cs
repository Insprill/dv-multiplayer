namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonHoseDisconnectedPacket
{
    public string CarGUID { get; set; }
    public bool IsFront { get; set; }
    public bool PlayAudio { get; set; }
}
