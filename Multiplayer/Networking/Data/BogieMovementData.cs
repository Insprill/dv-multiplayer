using LiteNetLib.Utils;
using Multiplayer.Components;

namespace Multiplayer.Networking.Packets.Common;

public struct BogieMovementData
{
    public bool IncludesTrackData { get; set; }
    public double PositionAlongTrack { get; set; }
    public ushort TrackIndex { get; set; }
    public int TrackDirection { get; set; }

    public static BogieMovementData FromBogie(Bogie bogie, bool includeTrack, int trackDirection)
    {
        return new BogieMovementData {
            IncludesTrackData = includeTrack && !bogie.HasDerailed,
            TrackIndex = includeTrack && !bogie.HasDerailed ? WorldComponentLookup.Instance.IndexFromTrack(bogie.track) : ushort.MaxValue,
            PositionAlongTrack = bogie.traveller?.Span ?? -1.0,
            TrackDirection = trackDirection
        };
    }

    public static void Serialize(NetDataWriter writer, BogieMovementData data)
    {
        writer.Put(data.IncludesTrackData);
        writer.Put(data.PositionAlongTrack);
        if (!data.IncludesTrackData) return;
        writer.Put(data.TrackIndex);
        writer.Put(data.TrackDirection);
    }

    public static BogieMovementData Deserialize(NetDataReader reader)
    {
        BogieMovementData data = new() {
            IncludesTrackData = reader.GetBool(),
            PositionAlongTrack = reader.GetDouble()
        };

        if (!data.IncludesTrackData)
            return data;

        data.TrackIndex = reader.GetUShort();
        data.TrackDirection = reader.GetInt();
        return data;
    }
}
