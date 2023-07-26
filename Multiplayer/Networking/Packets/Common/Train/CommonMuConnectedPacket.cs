namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonMuConnectedPacket
{
    public ushort NetId { get; set; }
    public bool IsFront { get; set; }
    public ushort OtherNetId { get; set; }
    public bool OtherIsFront { get; set; }
    public bool PlayAudio { get; set; }
}
