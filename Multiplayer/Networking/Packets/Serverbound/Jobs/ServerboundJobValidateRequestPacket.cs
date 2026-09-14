using Multiplayer.Networking.Data.Jobs;

namespace Multiplayer.Networking.Packets.Clientbound.Jobs;

public class ServerboundJobValidateRequestPacket
{
    public ushort JobNetId { get; set; }
    public uint StationNetId { get; set; }
    public ValidationType validationType { get; set; }
}
