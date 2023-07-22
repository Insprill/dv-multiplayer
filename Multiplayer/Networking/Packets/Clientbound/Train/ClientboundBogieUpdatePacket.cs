using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundBogieUpdatePacket
{
    public string CarGUID { get; set; }
    public BogieData Bogie1 { get; set; }
    public BogieData Bogie2 { get; set; }
}
