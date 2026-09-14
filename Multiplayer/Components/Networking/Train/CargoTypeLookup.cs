using DV;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Multiplayer.Components.Networking.Train;

public class CargoTypeLookup : SingletonBehaviour<CargoTypeLookup>
{
    private readonly Dictionary<uint, CargoType_v2> hashToCargoTypeV2 = [];
    private readonly Dictionary<CargoType_v2, uint> cargoTypeV2ToHash = [];

    protected override void Awake()
    {
        base.Awake();

        hashToCargoTypeV2.Clear();
        cargoTypeV2ToHash.Clear();

        RebuildCache();
    }

    protected void RebuildCache()
    {
        var missingCargoTypes = Globals.G.Types.cargos.Where(c => !cargoTypeV2ToHash.ContainsKey(c));

        if (!missingCargoTypes.Any())
            return;

        Multiplayer.LogDebug(() => $"CargoTypeLookup: Found {missingCargoTypes.Count()} missing cargo types, registering...");

        foreach (var cargoType in missingCargoTypes)
            TryGetNetId(cargoType, out _);
    }

    public bool TryGet(uint netId, out CargoType_v2 cargoType)
    {
        if (hashToCargoTypeV2.TryGetValue(netId, out cargoType))
            return true;

        RebuildCache();

        if (hashToCargoTypeV2.TryGetValue(netId, out cargoType))
            return true;

        Multiplayer.LogWarning($"CargoTypeLookup: Could not find CargoType for netId {netId}");

        cargoType = CargoType.None.ToV2();
        return false;
    }

    public bool TryGet(uint netId, out CargoType cargoType)
    {
        if (TryGet(netId, out CargoType_v2 cargoTypeV2))
        {
            cargoType = cargoTypeV2.v1;
            return true;
        }

        cargoType = CargoType.None;
        return false;
    }

    public bool TryGetNetId(CargoType_v2 cargoType, out uint netId)
    {
        netId = 0;
        if (cargoType == null)
            return false;

        if (cargoTypeV2ToHash.TryGetValue(cargoType, out netId))
            return true;

        uint hash = StringHashing.Fnv1aHash(cargoType.id);
        Multiplayer.LogDebug(()=> $"Registering cargo type '{cargoType.id}', netId: {hash}");

        if (hash == 0 || hash == uint.MaxValue)
        {
            Multiplayer.LogError($"Computed hash for cargo type '{cargoType.id}' is {hash}, which is reserved.");
            netId = 0;
            return false;
        }

        if (hashToCargoTypeV2.TryGetValue(hash, out var existingCargoType))
        {
            if (existingCargoType.id != cargoType.id)
            {
                Multiplayer.LogError($"Hash collision detected! Cargo type '{cargoType.id}' has same hash as '{existingCargoType.id}': {hash}.");
                netId = 0;
                return false;
            }
        }

        cargoTypeV2ToHash[cargoType] = hash;
        hashToCargoTypeV2[hash] = cargoType;
        netId = hash;

        //Multiplayer.Log($"CargoType '{cargoType.id}' registered with netId: {netId}");

        return true;
    }

    public bool TryGetNetId(CargoType cargoType, out uint netId)
    {
        return TryGetNetId(cargoType.ToV2(), out netId);
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(CargoTypeLookup)}]";
    }
}
