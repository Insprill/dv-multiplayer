using DV.HUD;

namespace Multiplayer.Networking.Packets.Serverbound.Train;

public class ServerboundTrainControlAuthorityPacket
{
    public ushort NetId { get; set; }
    public uint PortNetId { get; set; }
    public InteriorControlsManager.ControlType ControlType { get; set; }
    public bool RequestAuthority { get; set; }

}
