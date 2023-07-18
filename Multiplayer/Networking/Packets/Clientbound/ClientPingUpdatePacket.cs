namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientPingUpdatePacket
{
    public byte Id { get; set; }
    public int Ping { get; set; }
}
