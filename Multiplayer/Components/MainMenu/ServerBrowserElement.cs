using DV.Common;
using DV.Localization;
using DV.UIFramework;
using Multiplayer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Multiplayer.Components.MainMenu;


// 
public class ServerBrowserElement : AViewElement<IServerBrowserGameDetails>
{
    private TextMeshProUGUI networkName;
    private TextMeshProUGUI playerCount;
    private TextMeshProUGUI ping;
    private IServerBrowserGameDetails data;

    private void Awake()
    {
        //Find existing fields to duplicate
        networkName = this.FindChildByName("name [noloc]").GetComponent<TextMeshProUGUI>();
        playerCount = this.FindChildByName("date [noloc]").GetComponent<TextMeshProUGUI>();
        ping = this.FindChildByName("time [noloc]").GetComponent<TextMeshProUGUI>();

        networkName.text = "Test Network";
        playerCount.text = "1/4";
        ping.text = "102";
    }

    public override void SetData(IServerBrowserGameDetails data, AGridView<IServerBrowserGameDetails> _)
    {
        if (this.data != null)
        {
            this.data = null;
        }
        if (data != null)
        {
            this.data = data;
        }
        UpdateView(null, null);
    }

    // 
    private void UpdateView(object sender = null, PropertyChangedEventArgs e = null)
    {
        networkName.text = data.Name;
    }

}
