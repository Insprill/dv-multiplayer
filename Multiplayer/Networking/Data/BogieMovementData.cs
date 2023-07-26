using LiteNetLib.Utils;
using Multiplayer.Components;

namespace Multiplayer.Networking.Packets.Common;

public struct BogieMovementData
{
    public ushort TrackIndex { get; set; }
    public double PositionAlongTrack { get; set; }

    public static BogieMovementData FromBogie(Bogie bogie, bool includeTrack)
    {
        return new BogieMovementData {
            TrackIndex = includeTrack && !bogie.HasDerailed ? WorldComponentLookup.Instance.IndexFromTrack(bogie.track) : ushort.MaxValue,
            PositionAlongTrack = bogie.traveller?.Span ?? -1.0
        };
    }

    public static void Serialize(NetDataWriter writer, BogieMovementData data)
    {
        writer.Put(data.TrackIndex);
        writer.Put(data.PositionAlongTrack);
    }

    public static BogieMovementData Deserialize(NetDataReader reader)
    {
        return new BogieMovementData {
            TrackIndex = reader.GetUShort(),
            PositionAlongTrack = reader.GetDouble()
        };
    }
}
