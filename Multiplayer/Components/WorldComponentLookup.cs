using System;
using System.Collections.Generic;
using DV;
using DV.Utils;
using JetBrains.Annotations;

namespace Multiplayer.Components;

public class WorldComponentLookup : SingletonBehaviour<WorldComponentLookup>
{
    private readonly Dictionary<RailTrack, ushort> trackToIndex = new();

    public bool TrackFromIndex(ushort index, out RailTrack track)
    {
        RailTrack[] railTracks = WorldData.Instance.OrderedRailtracks;
        if (railTracks.Length > index)
        {
            track = railTracks[index];
            return true;
        }

        track = null;
        return false;
    }

    public ushort IndexFromTrack(RailTrack track)
    {
        if (trackToIndex.TryGetValue(track, out ushort index))
            return index;
        index = (ushort)Array.FindIndex(WorldData.Instance.OrderedRailtracks, j => j == track);
        trackToIndex[track] = index;
        return index;
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(WorldComponentLookup)}]";
    }
}
