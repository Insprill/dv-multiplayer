using System.Collections.Generic;
using DV;
using DV.ThingTypes;
using DV.Utils;
using JetBrains.Annotations;

namespace Multiplayer.Components;

public class TrainComponentLookup : SingletonBehaviour<TrainComponentLookup>
{
    private readonly Dictionary<string, TrainCarLivery> liveryIdToLivery = new();

    public bool LiveryFromId(string liveryId, out TrainCarLivery livery)
    {
        if (liveryIdToLivery.TryGetValue(liveryId, out livery))
            return true;
        TrainCarLivery l = Globals.G.Types.Liveries.Find(l => l.id == liveryId);
        if (l == null)
            return false;
        livery = l;
        liveryIdToLivery[liveryId] = l;
        return true;
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(TrainComponentLookup)}]";
    }
}
