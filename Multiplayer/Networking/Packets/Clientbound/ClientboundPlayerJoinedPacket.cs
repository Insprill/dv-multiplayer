namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundPlayerJoinedPacket
{
    public byte Id { get; set; }
    public string Username { get; set; }
}
