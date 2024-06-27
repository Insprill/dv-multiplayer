using System;
using System.Collections;
using System.Text.RegularExpressions;
using DV.Localization;
using DV.UI;
using DV.UIFramework;
using DV.Util;
using DV.Utils;
using Multiplayer.Components.Networking;
using Multiplayer.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using Multiplayer.Networking.Data;



namespace Multiplayer.Components.MainMenu;

public class HostGamePane : MonoBehaviour
{

   
    #region setup

    private void Awake()
    {
        Multiplayer.Log("HostGamePane Awake()");

        
        BuildUI();


    }

    private void OnEnable()
    {
        Multiplayer.Log("HostGamePane OnEnable()");
        this.SetupListeners(true);
    }

    // Disable listeners
    private void OnDisable()
    {
        this.SetupListeners(false);
    }

    private void BuildUI()
    {
            

    }

       
    private void SetupListeners(bool on)
    {
        if (on)
        {
            //this.gridView.SelectedIndexChanged += this.IndexChanged;
        }
        else
        {
            //this.gridView.SelectedIndexChanged -= this.IndexChanged;
        }

    }

    #endregion

    #region UI callbacks

    #endregion

       
}
