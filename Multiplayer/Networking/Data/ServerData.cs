using Multiplayer.Components.MainMenu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer.Networking.Data
{
    public class ServerData : IServerBrowserGameDetails
    {

        public string id { get; set; }  //not yet used
        public string ip { get; set; }
        public ushort port { get; set; }


        [JsonProperty("server_name")]
        public string Name { get; set; }


        [JsonProperty("password_protected")]
        public bool HasPassword { get; set; }


        [JsonProperty("game_mode")]
        public int GameMode { get; set; }


        [JsonProperty("difficulty")]
        public int Difficulty { get; set; }


        [JsonProperty("time_passed")]
        public string TimePassed { get; set; }


        [JsonProperty("current_players")]
        public int CurrentPlayers { get; set; }


        [JsonProperty("max_players")]
        public int MaxPlayers { get; set; }


        [JsonProperty("required_mods")]
        public string RequiredMods { get; set; }


        [JsonProperty("game_version")]
        public string GameVersion { get; set; }


        [JsonProperty("multiplayer_version")]
        public string MultiplayerVersion { get; set; }


        [JsonProperty("server_info")]
        public string ServerDetails { get; set; }

        public int Ping { get; set; }


        public void Dispose() { }
    }
}
