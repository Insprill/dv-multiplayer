namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonTrainFusesPacket
{
    public ushort NetId { get; set; }
    public string[] FuseIds { get; set; }
    public bool[] FuseValues { get; set; }
}
