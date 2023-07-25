using Multiplayer.Networking.Packets.Common;

namespace Multiplayer.Components.Networking.Train;

public class NetworkedBogie : TickedQueue<BogieMovementData>
{
    private Bogie bogie;

    protected override void OnEnable()
    {
        bogie = GetComponent<Bogie>();
        if (bogie == null)
        {
            Multiplayer.LogError($"{gameObject.name}: {nameof(NetworkedBogie)} requires a {nameof(Bogie)} component on the same GameObject!");
            return;
        }

        base.OnEnable();
    }

    protected override void Process(BogieMovementData snapshot)
    {
        if (snapshot.NewTrack != bogie.track.gameObject.name)
            bogie.SetTrack(RailTrackRegistry.Instance.GetTrackWithName(snapshot.NewTrack), snapshot.PositionAlongTrack, snapshot.TrackDirection);
        else
            bogie.traveller.MoveToSpan(snapshot.PositionAlongTrack);
        bogie.rb.velocity = snapshot.Velocity;
    }
}
