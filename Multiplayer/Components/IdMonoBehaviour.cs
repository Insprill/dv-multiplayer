using System.Collections.Generic;
using LiteNetLib.Utils;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Components;

public abstract class IdMonoBehaviour<T, I> : MonoBehaviour where T : struct where I : MonoBehaviour
{
    private static readonly IdPool<T> idPool = new();
    private static readonly Dictionary<T, IdMonoBehaviour<T, I>> indexToObject = new();

    public T NetId;
    protected abstract bool IsIdServerAuthoritative { get; }

    protected static bool Get(T netId, out IdMonoBehaviour<T, I> obj)
    {
        if (indexToObject.TryGetValue(netId, out obj))
            return true;
        obj = null;
        Multiplayer.LogDebug(() => $"Got invalid NetId {netId} while processing packet {NetPacketProcessor.CurrentlyProcessingPacket}");
        return false;
    }

    protected virtual void Awake()
    {
        if (IsIdServerAuthoritative && !NetworkLifecycle.Instance.IsHost())
            return;
        Register(idPool.NextId);
    }

    public void Register(T id)
    {
        NetId = id;
        indexToObject[id] = this;
    }

    protected virtual void OnDestroy()
    {
        idPool.ReleaseId(NetId);
        if (!UnloadWatcher.isUnloading)
            return;
        indexToObject.Clear();
    }
}
