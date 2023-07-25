using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Components.Networking;

public abstract class TickedQueue<T> : MonoBehaviour
{
    private const byte MAX_SNAPSHOTS = 5;

    private int lastTimestamp = int.MinValue;
    private readonly Queue<T> snapshots = new(MAX_SNAPSHOTS);

    protected virtual void OnEnable()
    {
        NetworkLifecycle.Instance.OnTick += OnTick;
    }

    protected virtual void OnDisable()
    {
        if (UnloadWatcher.isQuitting)
            return;
        NetworkLifecycle.Instance.OnTick -= OnTick;
        lastTimestamp = int.MinValue;
        snapshots.Clear();
    }

    public void ReceiveSnapshot(T snapshot, int timestamp)
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
        Process(snapshots.Dequeue());
    }

    protected abstract void Process(T snapshot);
}
