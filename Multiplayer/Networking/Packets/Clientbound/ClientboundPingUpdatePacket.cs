namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundPingUpdatePacket
{
    public byte Id { get; set; }
    public int Ping { get; set; }
}
