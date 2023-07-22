using LiteNetLib.Utils;

namespace Multiplayer.Networking.Packets.Common;

public struct BogieData
{
    public string Track { get; set; }
    public double PositionAlongTrack { get; set; }
    public bool IsDerailed { get; set; }

    public static BogieData FromBogie(Bogie bogie)
    {
        return new BogieData {
            Track = bogie.track.gameObject.name,
            PositionAlongTrack = bogie.traveller.pointRelativeSpan,
            IsDerailed = bogie.HasDerailed
        };
    }

    public static void Serialize(NetDataWriter writer, BogieData data)
    {
        writer.Put(data.Track);
        writer.Put(data.PositionAlongTrack);
        writer.Put(data.IsDerailed);
    }

    public static BogieData Deserialize(NetDataReader reader)
    {
        return new BogieData {
            Track = reader.GetString(),
            PositionAlongTrack = reader.GetDouble(),
            IsDerailed = reader.GetBool()
        };
    }
}
