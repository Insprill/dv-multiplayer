namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonHoseDisconnectedPacket
{
    public ushort NetId { get; set; }
    public bool IsFront { get; set; }
    public bool PlayAudio { get; set; }
}
