namespace Multiplayer.Networking.Packets.Serverbound;

public class CommonRotateTurntablePacket
{
    public byte index { get; set; }
    public float rotation { get; set; }
}
