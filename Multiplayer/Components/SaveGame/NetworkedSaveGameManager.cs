using DV.JObjectExtstensions;
using DV.Utils;
using JetBrains.Annotations;
using Multiplayer.Components.Networking;
using Multiplayer.Components.Networking.Player;
using Newtonsoft.Json.Linq;

namespace Multiplayer.Components.SaveGame;

public class NetworkedSaveGameManager : SingletonBehaviour<NetworkedSaveGameManager>
{
    private const string KEY = "Multiplayer";

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
        JObject json = data.GetJObject(KEY) ?? new JObject();

        foreach (NetworkedPlayer player in NetworkLifecycle.Instance.Client.PlayerManager.Players)
        {
            JObject playerData = new();
            playerData.SetVector3(SaveGameKeys.Player_position, player.transform.position - WorldMover.currentMove);
            playerData.SetFloat(SaveGameKeys.Player_rotation, player.transform.rotation.y);
            json.SetJObject($"Player_{player.Username}", playerData);
        }

        data.SetJObject(KEY, json);
    }

    public JObject GetPlayerData(SaveGameData data, string username)
    {
        return data?.GetJObject(KEY)?.GetJObject($"Player_{username}");
    }

    [UsedImplicitly]
    public new static string AllowAutoCreate()
    {
        return $"[{nameof(NetworkedSaveGameManager)}]";
    }
}
