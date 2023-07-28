namespace Multiplayer.Components.Networking.Train;

public class TrainSpeedQueue : TickedQueue<float>
{
    private TrainCar trainCar;

    protected override void OnEnable()
    {
        trainCar = GetComponent<TrainCar>();
        if (trainCar == null)
        {
            Multiplayer.LogError($"{gameObject.name}: {nameof(TrainSpeedQueue)} requires a {nameof(TrainCar)} component on the same GameObject!");
            return;
        }

        base.OnEnable();
    }

    protected override void Process(float snapshot, uint snapshotTick)
    {
        // TrainCar#SetForwardSpeed doesn't check for derailed bogies
        trainCar.rb.velocity = trainCar.transform.forward * snapshot;
        foreach (Bogie bogey in trainCar.Bogies)
            if (!bogey.HasDerailed)
                bogey.rb.velocity = bogey.transform.forward * snapshot;
    }
}
