using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class NetworkedTrainCar : MonoBehaviour
{
    private TrainCar trainCar;

    private void Awake()
    {
        trainCar = GetComponent<TrainCar>();
    }

    private void SendBogieUpdate()
    {
        NetworkLifecycle.Instance.Server.SendBogieUpdate(trainCar);
    }
}
