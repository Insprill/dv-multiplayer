using DV.JObjectExtstensions;
using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Player;
using Newtonsoft.Json.Linq;

namespace Multiplayer.Components.SaveGame;

public class NetworkedSaveGameManager : SingletonBehaviour<NetworkedSaveGameManager>
{
    protected override void Awake()
    {
        base.Awake();
        if (NetworkLifecycle.Instance.IsHost())
            return;
        Multiplayer.LogError($"{nameof(NetworkedSaveGameManager)} should only exist on the host! Destroying self.");
        Destroy(this);
    }

    public void UpdateInternalData(SaveGameData data)
    {
        JObject json = new();

        foreach (NetworkedPlayer player in NetworkLifecycle.Instance.Client.PlayerManager.Players)
        {
            JObject playerData = new();
            playerData.SetVector3("Position", player.transform.position - WorldMover.currentMove);
            playerData.SetFloat("Rotation", player.transform.rotation.y);
            json.SetJObject($"Player_{player.Username}", playerData);
        }

        data.SetJObject("Multiplayer", json);
    }

    public JObject GetPlayerData(SaveGameData data, string username)
    {
        return data?.GetJObject("Multiplayer")?.GetJObject($"Player_{username}");
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(NetworkedSaveGameManager)}]";
    }
}
