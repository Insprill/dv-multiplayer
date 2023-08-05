using DV.ServicePenalty;
using DV.Utils;
using Multiplayer.Components.Networking;

namespace Multiplayer.Components.SaveGame;

public class NetworkedCareerManagerDebtController : SingletonBehaviour<NetworkedCareerManagerDebtController>
{
    private CareerManagerDebtController controller;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<CareerManagerDebtController>();
        if (!NetworkLifecycle.Instance.IsHost())
            return;
        controller.DebtListsUpdated += Server_OnDebtListsUpdated;
    }

    protected override void OnDestroy()
    {
        if (UnloadWatcher.isUnloading)
            return;
        controller.DebtListsUpdated -= Server_OnDebtListsUpdated;
    }

    private void Server_OnDebtListsUpdated()
    {
        switch (controller.NumberOfNonZeroPricedDebts)
        {
            case 0:
                NetworkLifecycle.Instance.Server.SendDebtStatus(false);
                break;
            case 1:
                NetworkLifecycle.Instance.Server.SendDebtStatus(true);
                break;
        }
    }
}
