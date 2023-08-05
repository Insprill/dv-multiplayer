namespace Multiplayer.Networking.Packets.Clientbound.SaveGame;

public class ClientboundLicenseAcquiredPacket
{
    public string Id { get; set; }
    public bool IsJobLicense { get; set; }
}
