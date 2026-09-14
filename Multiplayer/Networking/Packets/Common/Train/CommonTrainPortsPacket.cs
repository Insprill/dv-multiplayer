namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonTrainPortsPacket
{
    public ushort NetId { get; set; }
    public uint[] PortIds { get; set; }
    public float[] PortValues { get; set; }
}
