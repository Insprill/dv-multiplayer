using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ModMismatchPacket
{
    public ModInfo[] Missing;
    public ModInfo[] Extra;
}
