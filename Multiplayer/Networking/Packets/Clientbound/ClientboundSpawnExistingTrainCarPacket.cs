using Multiplayer.Networking.Packets.Common;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundSpawnExistingTrainCarPacket
{
    public string Id { get; set; }
    public string CarId { get; set; }
    public string CarGuid { get; set; }
    public bool PlayerSpawnedCar { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public BogieData Bogie1 { get; set; }
    public BogieData Bogie2 { get; set; }
    public bool CouplerFCoupled { get; set; }
    public bool CouplerRCoupled { get; set; }

    public static ClientboundSpawnExistingTrainCarPacket FromTrainCar(TrainCar trainCar)
    {
        return new ClientboundSpawnExistingTrainCarPacket {
            Id = trainCar.carLivery.id,
            CarId = trainCar.ID,
            CarGuid = trainCar.CarGUID,
            PlayerSpawnedCar = trainCar.playerSpawnedCar,
            Position = trainCar.transform.position,
            Rotation = trainCar.transform.eulerAngles,
            Bogie1 = new BogieData {
                Track = trainCar.Bogies[0].track.gameObject.name,
                PositionAlongTrack = trainCar.Bogies[0].traveller.pointRelativeSpan,
                IsDerailed = trainCar.Bogies[0].HasDerailed
            },
            Bogie2 = new BogieData {
                Track = trainCar.Bogies[1].track.gameObject.name,
                PositionAlongTrack = trainCar.Bogies[1].traveller.pointRelativeSpan,
                IsDerailed = trainCar.Bogies[1].HasDerailed
            },
            CouplerFCoupled = trainCar.frontCoupler.IsCoupled(),
            CouplerRCoupled = trainCar.rearCoupler.IsCoupled()
        };
    }
}
