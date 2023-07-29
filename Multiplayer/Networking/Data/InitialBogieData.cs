using LiteNetLib.Utils;
using Multiplayer.Components;

namespace Multiplayer.Networking.Packets.Common;

public struct InitialBogieData
{
    public ushort Track { get; set; }
    public double PositionAlongTrack { get; set; }
    public bool IsDerailed { get; set; }

    public static InitialBogieData FromBogie(Bogie bogie)
    {
        return new InitialBogieData {
            Track = bogie.HasDerailed ? ushort.MaxValue : WorldComponentLookup.Instance.IndexFromTrack(bogie.track),
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
            Track = reader.GetUShort(),
            PositionAlongTrack = reader.GetDouble(),
            IsDerailed = reader.GetBool()
        };
    }
}
