using DV.LocoRestoration;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundRestorationStateChangePacket
{
    public ushort NetId { get; set; }
    public LocoRestorationController.RestorationState NewState { get; set; }
    public ushort[] TransportCarNetIds { get; set; }
}
