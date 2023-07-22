using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class NetworkedTrainCar : MonoBehaviour
{
    private TrainCar trainCar;

    private void Awake()
    {
        trainCar = GetComponent<TrainCar>();
        foreach (Coupler coupler in trainCar.couplers)
        {
            coupler.Coupled += OnCouple;
            coupler.Uncoupled += OnUncouple;
        }
    }

    private void OnCouple(object sender, CoupleEventArgs args)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        NetworkLifecycle.Instance.Client?.SendTrainCouple(args.thisCoupler, args.otherCoupler, args.viaChainInteraction);
    }

    private void OnUncouple(object sender, UncoupleEventArgs args)
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;
        NetworkLifecycle.Instance.Client?.SendTrainUncouple(args.thisCoupler, args.otherCoupler, args.viaChainInteraction);
    }

    private void SendBogieUpdate()
    {
        NetworkLifecycle.Instance.Server.SendBogieUpdate(trainCar);
    }
}
