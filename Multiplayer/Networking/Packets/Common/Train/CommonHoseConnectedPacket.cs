namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonHoseConnectedPacket
{
    public string CarGUID { get; set; }
    public bool IsFront { get; set; }
    public string OtherCarGUID { get; set; }
    public bool OtherIsFront { get; set; }
    public bool PlayAudio { get; set; }
}
