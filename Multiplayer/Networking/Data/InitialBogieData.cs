using LiteNetLib.Utils;

namespace Multiplayer.Networking.Packets.Common;

public struct InitialBogieData
{
    public string Track { get; set; }
    public double PositionAlongTrack { get; set; }
    public bool IsDerailed { get; set; }

    public static InitialBogieData FromBogie(Bogie bogie)
    {
        return new InitialBogieData {
            Track = !bogie.track ? string.Empty : bogie.track.gameObject.name,
            PositionAlongTrack = bogie.traveller?.Span ?? -1.0,
            IsDerailed = bogie.HasDerailed
        };
    }

    public static void Serialize(NetDataWriter writer, InitialBogieData data)
    {
        writer.Put(data.Track);
        writer.Put(data.PositionAlongTrack);
        writer.Put(data.IsDerailed);
    }

    public static InitialBogieData Deserialize(NetDataReader reader)
    {
        return new InitialBogieData {
            Track = reader.GetString(),
            PositionAlongTrack = reader.GetDouble(),
            IsDerailed = reader.GetBool()
        };
    }
}
