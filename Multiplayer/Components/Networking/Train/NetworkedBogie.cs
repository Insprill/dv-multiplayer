using Multiplayer.Networking.Packets.Common;
using UnityEngine;

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

    protected override void Process(BogieMovementData snapshot, uint snapshotTick)
    {
        if (bogie.HasDerailed)
            return;

        if (snapshot.TrackIndex != ushort.MaxValue)
        {
            if (WorldComponentLookup.Instance.TrackFromIndex(snapshot.TrackIndex, out RailTrack track))
            {
                bogie.SetTrack(track, snapshot.PositionAlongTrack);
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
