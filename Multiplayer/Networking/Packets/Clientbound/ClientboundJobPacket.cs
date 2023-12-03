using Multiplayer.Networking.Data;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundJobPacket
{
    public string StationId { get; set; }
    public JobData[] Jobs { get; set; }

    public ModInfo[] ModInfo { get; set; }
}
