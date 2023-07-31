using System;
using System.Collections.Generic;
using DV;
using DV.Utils;
using JetBrains.Annotations;

namespace Multiplayer.Components;

public class WorldComponentLookup : SingletonBehaviour<WorldComponentLookup>
{
    private readonly Dictionary<Junction, ushort> junctionToIndex = new();
    private readonly Dictionary<RailTrack, ushort> trackToIndex = new();

    public bool JunctionFromIndex(ushort index, out Junction junction)
    {
        Junction[] junctions = WorldData.Instance.OrderedJunctions;
        if (junctions.Length > index)
        {
            junction = junctions[index];
            return true;
        }

        junction = null;
        return false;
    }

    public ushort IndexFromJunction(Junction junction)
    {
        if (junctionToIndex.TryGetValue(junction, out ushort index))
            return index;
        index = (ushort)Array.FindIndex(WorldData.Instance.OrderedJunctions, j => j == junction);
        junctionToIndex[junction] = index;
        return index;
    }

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
