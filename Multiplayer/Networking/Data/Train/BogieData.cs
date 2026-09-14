using LiteNetLib.Utils;
using Multiplayer.Utils;
using System;

namespace Multiplayer.Networking.Data.Train;

[Flags]
public enum BogieFlags : byte
{
    None = 0,
    IncludesTrackData = 1,
    HasDerailed = 2,
    TrackReversed = 4
}
public readonly struct BogieData
{
    public readonly BogieFlags DataFlags;
    public readonly double PositionAlongTrack;
    public readonly ushort TrackNetId;

    public readonly int TrackDirection => DataFlags.HasFlag(BogieFlags.TrackReversed) ? -1 : 1;
    public readonly bool IncludesTrackData => DataFlags.HasFlag(BogieFlags.IncludesTrackData);
    public readonly bool HasDerailed => DataFlags.HasFlag(BogieFlags.HasDerailed);

    private BogieData(BogieFlags flags, double positionAlongTrack, ushort trackNetId)
    {
        // Prevent invalid state combinations
        if (flags.HasFlag(BogieFlags.HasDerailed))
            flags &= ~BogieFlags.IncludesTrackData; // Clear track data flag if derailed

        DataFlags = flags;
        PositionAlongTrack = positionAlongTrack;
        TrackNetId = trackNetId;
    }

    public static BogieData FromBogie(Bogie bogie)
    {
        // Guard against null bogie
        if (bogie == null)
        {
            Multiplayer.LogWarning("BogieData.FromBogie() called with null bogie!");
            return new BogieData(BogieFlags.HasDerailed, -1.0, 0);
        }

        bool includesTrackData = !bogie.HasDerailed && bogie.track;

        BogieFlags flags = BogieFlags.None;

        if (includesTrackData) flags |= BogieFlags.IncludesTrackData;
        if (bogie.HasDerailed) flags |= BogieFlags.HasDerailed;
        if (bogie.trackDirection == -1) flags |= BogieFlags.TrackReversed;

        return new BogieData(
            flags,
            bogie.traveller?.Span ?? -1.0,
            includesTrackData ? bogie.track.Networked().NetId : (ushort)0
        );
    }

    public static void Serialize(NetDataWriter writer, BogieData data)
    {
        writer.Put((byte)data.DataFlags);

        if (!data.HasDerailed)
            writer.Put(data.PositionAlongTrack);

        if (data.IncludesTrackData)
            writer.Put(data.TrackNetId);
    }

    public static BogieData Deserialize(NetDataReader reader)
    {
        BogieFlags flags = (BogieFlags)reader.GetByte();

        // Read position if not derailed
        double positionAlongTrack = !flags.HasFlag(BogieFlags.HasDerailed)
            ? reader.GetDouble()
            : -1.0;

        // Read track data if included
        ushort trackNetId = 0;
        if (flags.HasFlag(BogieFlags.IncludesTrackData))
            trackNetId = reader.GetUShort();

        return new BogieData(flags, positionAlongTrack, trackNetId);
    }
}
