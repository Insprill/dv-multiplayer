using LiteNetLib.Utils;
using Multiplayer.Components;

namespace Multiplayer.Networking.Data;

public readonly struct BogieData
{
    private readonly byte PackedBools;
    public readonly double PositionAlongTrack;
    public readonly ushort TrackIndex;
    public readonly int TrackDirection;

    public bool IncludesTrackData => (PackedBools & 1) != 0;
    public bool HasDerailed => (PackedBools & 2) != 0;

    private BogieData(byte packedBools, double positionAlongTrack, ushort trackIndex, int trackDirection)
    {
        PackedBools = packedBools;
        PositionAlongTrack = positionAlongTrack;
        TrackIndex = trackIndex;
        TrackDirection = trackDirection;
    }

    public static BogieData FromBogie(Bogie bogie, bool includeTrack, int trackDirection)
    {
        return new BogieData(
            (byte)((includeTrack && !bogie.HasDerailed ? 1 : 0) | (bogie.HasDerailed ? 2 : 0)),
            bogie.traveller?.Span ?? -1.0,
            includeTrack && !bogie.HasDerailed ? WorldComponentLookup.Instance.IndexFromTrack(bogie.track) : ushort.MaxValue,
            trackDirection
        );
    }

    public static void Serialize(NetDataWriter writer, BogieData data)
    {
        writer.Put(data.PackedBools);
        if (!data.HasDerailed) writer.Put(data.PositionAlongTrack);
        if (!data.IncludesTrackData) return;
        writer.Put(data.TrackIndex);
        writer.Put(data.TrackDirection);
    }

    public static BogieData Deserialize(NetDataReader reader)
    {
        byte packedBools = reader.GetByte();
        bool includesTrackData = (packedBools & 1) != 0;
        bool hasDerailed = (packedBools & 2) != 0;
        double positionAlongTrack = !hasDerailed ? reader.GetDouble() : -1.0;
        ushort trackIndex = includesTrackData ? reader.GetUShort() : ushort.MaxValue;
        int trackDirection = includesTrackData ? reader.GetInt() : 0;
        return new BogieData(packedBools, positionAlongTrack, trackIndex, trackDirection);
    }
}
