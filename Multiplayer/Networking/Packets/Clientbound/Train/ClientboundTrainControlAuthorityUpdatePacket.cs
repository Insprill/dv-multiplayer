
using DV.HUD;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public enum ControlAuthorityState : byte
{
    Released,
    Blocked,
    Denied
}

public class ClientboundTrainControlAuthorityUpdatePacket
{
    public ushort NetId { get; set; }
    public uint PortNetId { get; set; }
    public ControlAuthorityState State { get; set; }
}
