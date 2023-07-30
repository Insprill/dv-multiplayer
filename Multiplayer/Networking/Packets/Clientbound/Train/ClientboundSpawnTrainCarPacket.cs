using Multiplayer.Networking.Packets.Common;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound.Train;

public class ClientboundSpawnTrainCarPacket
{
    public ushort NetId { get; set; }
    public string LiveryId { get; set; }
    public string CarId { get; set; }
    public string CarGuid { get; set; }
    public bool PlayerSpawnedCar { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public InitialBogieData Bogie1 { get; set; }
    public InitialBogieData Bogie2 { get; set; }
    public bool CouplerFCoupled { get; set; }
    public bool CouplerRCoupled { get; set; }

    public static ClientboundSpawnTrainCarPacket FromTrainCar(TrainCar trainCar)
    {
        return new ClientboundSpawnTrainCarPacket {
            NetId = trainCar.GetNetId(),
            LiveryId = trainCar.carLivery.id,
            CarId = trainCar.ID,
            CarGuid = trainCar.CarGUID,
            PlayerSpawnedCar = trainCar.playerSpawnedCar,
            Position = trainCar.transform.position - WorldMover.currentMove,
            Rotation = trainCar.transform.eulerAngles,
            Bogie1 = InitialBogieData.FromBogie(trainCar.Bogies[0]),
            Bogie2 = InitialBogieData.FromBogie(trainCar.Bogies[1]),
            CouplerFCoupled = trainCar.frontCoupler.IsCoupled(),
            CouplerRCoupled = trainCar.rearCoupler.IsCoupled()
        };
    }
}
