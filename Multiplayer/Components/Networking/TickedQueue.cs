using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Components.Networking;

public abstract class TickedQueue<T> : MonoBehaviour
{
    private const byte MAX_SNAPSHOTS = 5;

    private uint lastTick;
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
        lastTick = 0;
        snapshots.Clear();
    }

    public void ReceiveSnapshot(T snapshot, uint tick)
    {
        if (tick <= lastTick)
            return;
        lastTick = tick;
        if (snapshots.Count == MAX_SNAPSHOTS)
            snapshots.Dequeue();
        snapshots.Enqueue(snapshot);
    }

    private void OnTick(uint tick)
    {
        if (snapshots.Count == 0)
            return;
        Process(snapshots.Dequeue());
    }

    protected abstract void Process(T snapshot);
}
