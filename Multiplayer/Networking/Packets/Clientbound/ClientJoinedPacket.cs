using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientJoinedPacket
{
    public byte Id { get; set; }
    public string Username { get; set; }
}
