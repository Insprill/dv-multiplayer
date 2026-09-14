
namespace Multiplayer.Networking.Packets.Common;

public enum CashRegisterAction : byte
{
    Cancel,
    Buy,
    AddCash,
    SetFunds,
    RejectGeneric,
    RejectFunds,
    RejectedNoItems,
    Approve
}
public class CommonCashRegisterWithModulesActionPacket
{
    public ushort NetId { get; set; }
    public CashRegisterAction Action { get; set; }
    public double Amount { get; set; }
}
