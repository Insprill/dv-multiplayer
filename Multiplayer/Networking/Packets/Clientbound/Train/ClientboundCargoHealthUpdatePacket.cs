namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundCargoHealthUpdatePacket
{
    public ushort NetId { get; set; }
    public float CargoHealth { get; set; }
}
