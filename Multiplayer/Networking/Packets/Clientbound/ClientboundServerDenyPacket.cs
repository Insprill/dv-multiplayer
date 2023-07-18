using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundServerDenyPacket
{
    public string Reason { get; set; }
    public ModInfo[] Missing { get; set; }
    public ModInfo[] Extra { get; set; }
}
