using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DV.Common;
using DV.UI;
using DV.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Components.MainMenu
{
    [RequireComponent(typeof(ContentSizeFitter))]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    // 
    public class ServerBrowserGridView : AGridView<IServerBrowserGameDetails>
    {
         
        private void Awake()
        {
            Debug.Log("serverBrowserGridview Awake");

            this.dummyElementPrefab.SetActive(false);

            //swap controller
            GameObject.Destroy(this.dummyElementPrefab.GetComponent<SaveLoadViewElement>());
            this.dummyElementPrefab.AddComponent<ServerBrowserElement>();

            this.dummyElementPrefab.SetActive(true);
            this.viewElementPrefab = this.dummyElementPrefab;
            // GameObject defaultPrefab = GameObject.Find("SaveLoadViewElement");
            // this.dummyElementPrefab = Instantiate(defaultPrefab);
        }
    }
}
