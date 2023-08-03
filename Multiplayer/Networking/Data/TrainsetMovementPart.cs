using LiteNetLib.Utils;

namespace Multiplayer.Networking.Data;

public readonly struct TrainsetMovementPart
{
    public readonly float Speed;
    public readonly BogieData Bogie1;
    public readonly BogieData Bogie2;

    public TrainsetMovementPart(float speed, BogieData bogie1, BogieData bogie2)
    {
        Speed = speed;
        Bogie1 = bogie1;
        Bogie2 = bogie2;
    }

    public static void Serialize(NetDataWriter writer, TrainsetMovementPart data)
    {
        writer.Put(data.Speed);
        BogieData.Serialize(writer, data.Bogie1);
        BogieData.Serialize(writer, data.Bogie2);
    }

    public static TrainsetMovementPart Deserialize(NetDataReader reader)
    {
        return new TrainsetMovementPart(
            reader.GetFloat(),
            BogieData.Deserialize(reader),
            BogieData.Deserialize(reader)
        );
    }
}
