using System;
using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Serverbound;

[Serializable]
public class ServerboundClientInfoPacket
{
    public string Username { get; set; }
    public ushort BuildMajorVersion { get; set; }
    public ModInfo[] Mods { get; set; }
}
