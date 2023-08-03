using LiteNetLib.Utils;
using Multiplayer.Components.Networking.Train;
using Multiplayer.Networking.Serialization;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Networking.Data;

public struct TrainsetSpawnPart
{
    public ushort NetId { get; set; }
    public string LiveryId { get; set; }
    public string CarId { get; set; }
    public string CarGuid { get; set; }
    public bool PlayerSpawnedCar { get; set; }
    public bool IsFrontCoupled { get; set; }
    public bool IsRearCoupled { get; set; }
    public float Speed { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public BogieData Bogie1 { get; set; }
    public BogieData Bogie2 { get; set; }

    public static void Serialize(NetDataWriter writer, TrainsetSpawnPart data)
    {
        writer.Put(data.NetId);
        writer.Put(data.LiveryId);
        writer.Put(data.CarId);
        writer.Put(data.CarGuid);
        writer.Put(data.PlayerSpawnedCar);
        writer.Put(data.IsFrontCoupled);
        writer.Put(data.IsRearCoupled);
        writer.Put(data.Speed);
        Vector3Serializer.Serialize(writer, data.Position);
        Vector3Serializer.Serialize(writer, data.Rotation);
        BogieData.Serialize(writer, data.Bogie1);
        BogieData.Serialize(writer, data.Bogie2);
    }

    public static TrainsetSpawnPart Deserialize(NetDataReader reader)
    {
        return new TrainsetSpawnPart {
            NetId = reader.GetUShort(),
            LiveryId = reader.GetString(),
            CarId = reader.GetString(),
            CarGuid = reader.GetString(),
            PlayerSpawnedCar = reader.GetBool(),
            IsFrontCoupled = reader.GetBool(),
            IsRearCoupled = reader.GetBool(),
            Speed = reader.GetFloat(),
            Position = Vector3Serializer.Deserialize(reader),
            Rotation = Vector3Serializer.Deserialize(reader),
            Bogie1 = BogieData.Deserialize(reader),
            Bogie2 = BogieData.Deserialize(reader)
        };
    }

    public static TrainsetSpawnPart FromTrainCar(NetworkedTrainCar networkedTrainCar)
    {
        TrainCar trainCar = networkedTrainCar.TrainCar;
        Transform transform = networkedTrainCar.transform;
        return new TrainsetSpawnPart {
            NetId = networkedTrainCar.NetId,
            LiveryId = trainCar.carLivery.id,
            CarId = trainCar.ID,
            CarGuid = trainCar.CarGUID,
            PlayerSpawnedCar = trainCar.playerSpawnedCar,
            IsFrontCoupled = trainCar.frontCoupler.IsCoupled(),
            IsRearCoupled = trainCar.rearCoupler.IsCoupled(),
            Speed = trainCar.GetForwardSpeed(),
            Position = transform.position - WorldMover.currentMove,
            Rotation = transform.eulerAngles,
            Bogie1 = BogieData.FromBogie(trainCar.Bogies[0], true, networkedTrainCar.Bogie1TrackDirection),
            Bogie2 = BogieData.FromBogie(trainCar.Bogies[1], true, networkedTrainCar.Bogie2TrackDirection)
        };
    }

    public static TrainsetSpawnPart[] FromTrainSet(Trainset trainset)
    {
        TrainsetSpawnPart[] parts = new TrainsetSpawnPart[trainset.cars.Count];
        for (int i = 0; i < trainset.cars.Count; i++)
            parts[i] = FromTrainCar(trainset.cars[i].GetNetworkedCar());
        return parts;
    }
}
