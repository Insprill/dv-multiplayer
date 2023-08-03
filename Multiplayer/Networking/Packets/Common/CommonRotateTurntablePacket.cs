namespace Multiplayer.Networking.Packets.Common;

public class CommonRotateTurntablePacket
{
    public byte NetId { get; set; }
    public float rotation { get; set; }
}
