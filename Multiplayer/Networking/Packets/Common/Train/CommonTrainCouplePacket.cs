namespace Multiplayer.Networking.Packets.Common.Train;

public class CommonTrainCouplePacket
{
    public string CarGUID { get; set; }
    public bool IsFrontCoupler { get; set; }
    public string OtherCarGUID { get; set; }
    public bool OtherCarIsFrontCoupler { get; set; }
    public bool PlayAudio { get; set; }
    public bool ViaChainInteraction { get; set; }
}
