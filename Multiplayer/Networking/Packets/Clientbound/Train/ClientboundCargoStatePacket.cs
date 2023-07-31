namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundCargoStatePacket
{
    public ushort NetId { get; set; }
    public bool IsLoading { get; set; }
    public ushort CargoType { get; set; }
    public float CargoAmount { get; set; }
    public byte CargoModelIndex { get; set; }
    public string WarehouseMachineId { get; set; }
}
