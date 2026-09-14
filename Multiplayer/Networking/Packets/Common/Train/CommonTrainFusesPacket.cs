namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonTrainFusesPacket
{
    public ushort NetId { get; set; }
    public uint[] FuseIds { get; set; }
    public bool[] FuseValues { get; set; }
}
