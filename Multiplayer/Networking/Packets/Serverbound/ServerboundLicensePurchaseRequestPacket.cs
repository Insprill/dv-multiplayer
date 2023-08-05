namespace Multiplayer.Networking.Packets.Serverbound;

public class ServerboundLicensePurchaseRequestPacket
{
    public string Id { get; set; }
    public bool IsJobLicense { get; set; }
}
