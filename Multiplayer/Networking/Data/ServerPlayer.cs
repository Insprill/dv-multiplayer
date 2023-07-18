namespace Multiplayer.Networking.Packets.Common;

public class ServerPlayer
{
    public byte Id { get; set; }
    public string Username { get; set; }
    public int Ping { get; set; }
}
