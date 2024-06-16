using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Multiplayer.Components.MainMenu
{
    // 
    public interface IServerBrowserGameDetails : IDisposable
    {
        //
        // 
        int ServerID { get; }

        // 
        //
        // 
        string Name { get; set; }
        int MaxPlayers { get; set; }
        int CurrentPlayers { get; set; }
        int Ping { get; set; }
        bool HasPassword { get; set; }
    }
}
