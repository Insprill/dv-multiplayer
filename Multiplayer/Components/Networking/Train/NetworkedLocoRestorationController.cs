using DV.LocoRestoration;
using DV.ThingTypes;
using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Utils;
using System.Collections;
using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class NetworkRestorationWatcher : SingletonBehaviour<NetworkRestorationWatcher>
{

    public void AddController(LocoRestorationController controller)
    {
        if (controller == null)
            return;

        controller.StateChanged += HandleRestorationStateChange;

    }

    private static void HandleRestorationStateChange(LocoRestorationController controller, TrainCarLivery livery, LocoRestorationController.RestorationState newState)
    {
        ushort[] transportCars = null;

        var netId = controller.loco.GetNetId();

        switch (newState)
        {
            // Handle events that need comms to clients
            case LocoRestorationController.RestorationState.S5_PartOrdered:
                break;

            case LocoRestorationController.RestorationState.S6_PartPickedUp:
                transportCars = new ushort[controller.transportingCars.Count];

                for (int i = 0; i < controller.transportingCars.Count; i++)
                    transportCars[i] = controller.transportingCars[i].GetNetId();

                break;

            case LocoRestorationController.RestorationState.S7_PartDelivered:
            case LocoRestorationController.RestorationState.S8_PartInstalled:
            case LocoRestorationController.RestorationState.S9_LocoServiced:
                break;

            case LocoRestorationController.RestorationState.S10_PaintJobDone:
                CoroutineManager.Instance.StartCoroutine(WaitPaintDone(netId));
                return;

            default:
                return; // No need to send updates for other states
        }

        NetworkLifecycle.Instance.Server.SendRestorationStateChange(netId, newState, transportCars);
    }

    private static IEnumerator WaitPaintDone(ushort netId)
    {
        yield return new WaitForSecondsRealtime(0.25f);
        NetworkLifecycle.Instance.Server.SendRestorationStateChange(netId, LocoRestorationController.RestorationState.S10_PaintJobDone, null);
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        // Only create this component on the host
        if (NetworkLifecycle.Instance.IsHost())
            return $"[{nameof(NetworkRestorationWatcher)}]";

        return null;
    }
}
