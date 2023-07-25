namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonSimFlowPacket
{
    public ushort NetId { get; set; }
    public string[] PortIds { get; set; }
    public float[] PortValues { get; set; }
    public string[] FuseIds { get; set; }
    public bool[] FuseValues { get; set; }
}
