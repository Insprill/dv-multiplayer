using Multiplayer.Networking.Data;
using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public class NetworkedBogie : TickedQueue<BogieData>
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

    protected override void Process(BogieData snapshot, uint snapshotTick)
    {
        if (bogie.HasDerailed)
            return;

        if (snapshot.HasDerailed)
        {
            bogie.Derail();
            return;
        }

        if (snapshot.IncludesTrackData)
        {
            if (WorldComponentLookup.Instance.TrackFromIndex(snapshot.TrackIndex, out RailTrack track))
            {
                bogie.SetTrack(track, snapshot.PositionAlongTrack, snapshot.TrackDirection);
            }
            else
            {
                Multiplayer.LogError($"Could not find track with index {snapshot.TrackIndex}! Skipping update and waiting for the next snapshot.");
                return;
            }
        }
        else
        {
            bogie.traveller.MoveToSpan(snapshot.PositionAlongTrack);
        }

        int physicsSteps = Mathf.FloorToInt((NetworkLifecycle.Instance.Tick - (float)snapshotTick) / NetworkLifecycle.TICK_RATE / Time.fixedDeltaTime) + 1;
        for (int i = 0; i < physicsSteps; i++)
            bogie.FixedUpdate(); // 💀
    }
}
