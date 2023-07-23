using System.Collections.Generic;
using Multiplayer.Networking.Packets.Common;
using UnityEngine;

namespace Multiplayer.Components.Networking.World;

public class NetworkedRigidbody : MonoBehaviour
{
    private const byte MAX_SNAPSHOTS = 5;

    private Rigidbody rigidbody;

    private int lastTimestamp = int.MinValue;
    private readonly Queue<RigidbodySnapshot> snapshots = new(MAX_SNAPSHOTS);

    private void OnEnable()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Multiplayer.LogError($"{gameObject.name}: {nameof(NetworkedRigidbody)} requires a {nameof(Rigidbody)} component on the same GameObject!");
            return;
        }

        NetworkLifecycle.Instance.OnTick += OnTick;
    }

    private void OnDisable()
    {
        if (UnloadWatcher.isQuitting)
            return;
        NetworkLifecycle.Instance.OnTick -= OnTick;
        lastTimestamp = int.MinValue;
        snapshots.Clear();
    }

    public void ReceiveSnapshot(RigidbodySnapshot snapshot, int timestamp)
    {
        if (timestamp <= lastTimestamp)
            return;
        lastTimestamp = timestamp;
        if (snapshots.Count == MAX_SNAPSHOTS)
            snapshots.Dequeue();
        snapshots.Enqueue(snapshot);
    }

    private void OnTick()
    {
        if (snapshots.Count == 0)
            return;
        RigidbodySnapshot snapshot = snapshots.Dequeue();
        snapshot.Apply(rigidbody);
    }
}
