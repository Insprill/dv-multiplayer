namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonTrainUncouplePacket
{
    public string CarGUID { get; set; }
    public bool IsFrontCoupler { get; set; }
    public bool FromChainInteraction { get; set; }
    public bool DueToBrokenCouple { get; set; }
}
