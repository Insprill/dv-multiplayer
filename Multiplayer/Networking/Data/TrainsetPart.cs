using LiteNetLib.Utils;

namespace Multiplayer.Networking.Data;

public struct TrainsetPart
{
    public float Speed { get; set; }
    public BogieMovementData Bogie1 { get; set; }
    public BogieMovementData Bogie2 { get; set; }

    public static void Serialize(NetDataWriter writer, TrainsetPart data)
    {
        writer.Put(data.Speed);
        BogieMovementData.Serialize(writer, data.Bogie1);
        BogieMovementData.Serialize(writer, data.Bogie2);
    }

    public static TrainsetPart Deserialize(NetDataReader reader)
    {
        return new TrainsetPart {
            Speed = reader.GetFloat(),
            Bogie1 = BogieMovementData.Deserialize(reader),
            Bogie2 = BogieMovementData.Deserialize(reader)
        };
    }
}
