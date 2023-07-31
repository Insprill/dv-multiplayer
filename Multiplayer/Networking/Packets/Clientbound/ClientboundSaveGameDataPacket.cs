using DV.InventorySystem;
using DV.JObjectExtstensions;
using DV.UserManagement;
using Multiplayer.Components.Networking;
using Multiplayer.Components.SaveGame;
using Multiplayer.Networking.Packets.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound;

public class ClientboundSaveGameDataPacket
{
    public string GameMode { get; set; }
    public string World { get; set; }
    public string SerializedDifficulty { get; set; }
    public float Money { get; set; }
    public string[] AcquiredGeneralLicenses { get; set; }
    public string[] AcquiredJobLicenses { get; set; }
    public string[] UnlockedGarages { get; set; }
    public Vector3 Position { get; set; }
    public float Rotation { get; set; }

    public static ClientboundSaveGameDataPacket CreatePacket(ServerPlayer player)
    {
        if (WorldStreamingInit.isLoaded)
            SaveGameManager.Instance.UpdateInternalData();

        SaveGameData data = SaveGameManager.Instance.data;

        JObject difficulty = new();
        DifficultyDataUtils.SetDifficultyToJSON(difficulty, NetworkLifecycle.Instance.Server.Difficulty);

        JObject playerData = NetworkedSaveGameManager.Instance.GetPlayerData(data, player.Username);

        return new ClientboundSaveGameDataPacket {
            GameMode = data.GetString("Game_mode"),
            World = data.GetString("World"),
            SerializedDifficulty = difficulty.ToString(Formatting.None),
            Money = Inventory.Instance == null ? data.GetFloat("Player_money").GetValueOrDefault(0) : (float)Inventory.Instance.PlayerMoney,
            AcquiredGeneralLicenses = data.GetStringArray("Licenses_General"),
            AcquiredJobLicenses = data.GetStringArray("Licenses_Jobs"),
            UnlockedGarages = data.GetStringArray("Garages"),
            Position = playerData?.GetVector3("Position") ?? LevelInfo.DefaultSpawnPosition,
            Rotation = playerData?.GetFloat("Rotation") ?? LevelInfo.DefaultSpawnRotation.y
        };
    }
}
