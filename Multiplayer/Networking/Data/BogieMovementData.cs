using LiteNetLib.Utils;
using Multiplayer.Networking.Serialization;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Common;

public struct BogieMovementData
{
    public string NewTrack { get; set; }
    public int TrackDirection { get; set; }
    public double PositionAlongTrack { get; set; }
    public Vector3 Velocity { get; set; }

    public static BogieMovementData FromBogie(Bogie bogie, bool includeTrack)
    {
        return new BogieMovementData {
            NewTrack = includeTrack && bogie.track ? bogie.track.gameObject.name : string.Empty,
            TrackDirection = bogie.trackDirection,
            PositionAlongTrack = bogie.traveller?.Span ?? -1.0,
            Velocity = bogie.rb.velocity
        };
    }

    public static void Serialize(NetDataWriter writer, BogieMovementData data)
    {
        writer.Put(data.NewTrack);
        writer.Put(data.TrackDirection);
        writer.Put(data.PositionAlongTrack);
        Vector3Serializer.Serialize(writer, data.Velocity);
    }

    public static BogieMovementData Deserialize(NetDataReader reader)
    {
        return new BogieMovementData {
            NewTrack = reader.GetString(),
            TrackDirection = reader.GetInt(),
            PositionAlongTrack = reader.GetDouble(),
            Velocity = Vector3Serializer.Deserialize(reader)
        };
    }
}
