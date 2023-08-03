namespace Multiplayer.Networking.Packets.Clientbound.World;

public class ClientboundRailwayStatePacket
{
    public byte[] SelectedJunctionBranches { get; set; }
    public float[] TurntableRotations { get; set; }
}
