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
        if (bogie.HasDerailed)
            return;

        if (snapshot.TrackIndex != ushort.MaxValue)
        {
            if (WorldComponentLookup.Instance.TrackFromIndex(snapshot.TrackIndex, out RailTrack track))
                bogie.SetTrack(track, snapshot.PositionAlongTrack);
            else
                Multiplayer.LogError($"Could not find track with index {snapshot.TrackIndex}! Skipping update and waiting for next snapshot.");
        }
        else
        {
            // Vector3d worldPos = bogie.traveller.worldPosition;
            // bogie.traveller.MoveToSpan(snapshot.PositionAlongTrack);
            // Vector3d newWorldPos = bogie.traveller.worldPosition;
            // Multiplayer.LogDebug(() => $"Difference: {Vector3d.Distance(worldPos, newWorldPos)}");
        }
    }
}
