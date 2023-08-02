using System.Collections.Generic;
using DV.ThingTypes;
using Multiplayer.Networking.Packets.Clientbound.Train;
using Multiplayer.Utils;
using UnityEngine;

namespace Multiplayer.Components.Networking.Train;

public static class NetworkedCarSpawner
{
    public static void SpawnCar(ClientboundSpawnTrainCarPacket packet, TrainCarLivery livery, RailTrack bogie1Track, RailTrack bogie2Track)
    {
        (TrainCar trainCar, bool isPooled) = GetFromPool(livery);

        trainCar.gameObject.GetOrAddComponent<NetworkedTrainCar>().NetId = packet.NetId;
        trainCar.gameObject.GetOrAddComponent<TrainSpeedQueue>();

        trainCar.gameObject.SetActive(true);

        if (isPooled)
            trainCar.AwakeForPooledCar();

        Transform trainTransform = trainCar.transform;
        trainTransform.position = packet.Position + WorldMover.currentMove;
        trainTransform.eulerAngles = packet.Rotation;
        trainCar.playerSpawnedCar = packet.PlayerSpawnedCar;
        trainCar.InitializeExistingLogicCar(packet.CarId, packet.CarGuid);
        trainCar.preventAutoCouple = true;

        if (!packet.Bogie1.IsDerailed)
            trainCar.Bogies[0].SetTrack(bogie1Track, packet.Bogie1.PositionAlongTrack);
        else
            trainCar.Bogies[0].SetDerailedOnLoadFlag(true);

        if (!packet.Bogie2.IsDerailed)
            trainCar.Bogies[1].SetTrack(bogie2Track, packet.Bogie2.PositionAlongTrack);
        else
            trainCar.Bogies[1].SetDerailedOnLoadFlag(true);

        CarSpawner.Instance.FireCarSpawned(trainCar);
    }

    private static (TrainCar, bool) GetFromPool(TrainCarLivery livery)
    {
        if (!CarSpawner.Instance.useCarPooling || !CarSpawner.Instance.carLiveryToTrainCarPool.TryGetValue(livery, out List<TrainCar> trainCarList))
            return Instantiate(livery);

        int count = trainCarList.Count;
        if (count <= 0)
            return Instantiate(livery);

        int index = count - 1;
        TrainCar trainCar = trainCarList[index];
        trainCarList.RemoveAt(index);
        CarSpawner.Instance.trainCarPoolHashSet.Remove(trainCar);

        if (trainCar != null)
        {
            Transform trainCarTransform = trainCar.transform;
            trainCarTransform.SetParent(null);
            trainCarTransform.localScale = Vector3.one;
            trainCar.gameObject.SetActive(false); // Enabled after NetworkedTrainCar has been added

            Transform interiorTransform = trainCar.interior.transform;
            interiorTransform.SetParent(null);
            interiorTransform.localScale = Vector3.one;

            trainCar.interior.gameObject.SetActive(true);
            trainCar.rb.isKinematic = false;
            return (trainCar, true);
        }

        Multiplayer.LogError($"Failed to get {livery.id} from pool!");
        return Instantiate(livery);
    }

    private static (TrainCar, bool) Instantiate(TrainCarLivery livery)
    {
        bool wasActive = livery.prefab.activeSelf;
        livery.prefab.SetActive(false);
        (TrainCar, bool) result = (Object.Instantiate(livery.prefab).GetComponent<TrainCar>(), false);
        livery.prefab.SetActive(wasActive);
        return result;
    }
}
