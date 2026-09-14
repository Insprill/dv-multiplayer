using DV.Interaction;
using Multiplayer.Networking.Data;
using Multiplayer.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Components.Networking.World;

public class NetworkedGenericSwitch : MonoBehaviour
{
    #region lookup cache
    private static readonly Dictionary<uint, NetworkedGenericSwitch> netIdtoNetworked = [];
    private static readonly Dictionary<NetworkedGenericSwitch, uint> networkedToNetId = [];
    private static readonly Dictionary<GenericSwitch, uint> genericSwitchToNetId = [];

    public static IEnumerable<NetworkedGenericSwitch> AllSwitches => netIdtoNetworked.Values;
    public static bool TryGet(uint netId, out NetworkedGenericSwitch netSwitch)
    {
        return netIdtoNetworked.TryGetValue(netId, out netSwitch);
    }

    public static bool TryGetNetId(NetworkedGenericSwitch netSwitch, out uint netId)
    {
        return networkedToNetId.TryGetValue(netSwitch, out netId);
    }

    public static bool TryGetNetId(GenericSwitch genericSwitch, out uint netId)
    {
        return genericSwitchToNetId.TryGetValue(genericSwitch, out netId);
    }


    #endregion

    public uint NetId { get; private set; }
    public GenericSwitch Switch { get; private set; }

    public bool IsOn => Switch?.IsOn ?? false;



    protected void Awake()
    {
        
        Switch = GetComponent<GenericSwitch>();
        if (Switch == null)
        {
            Multiplayer.LogError($"NetworkedGenericSwitch.Awake() {nameof(GenericSwitch)} not found.");
            return;
        }

        GenerateNetId();

        Switch.onTurnedOff.AddListener(OnSwitchValueChanged);
        Switch.onTurnedOn.AddListener(OnSwitchValueChanged);

        Multiplayer.LogDebug(()=>$"NetworkedGenericSwitch.Awake() Persistence Key: \"{Switch.persistenceKey}\", netId: {NetId}");
    }

    protected void OnDestroy()
    {
        if (Switch != null)
        {
            Switch.onTurnedOff.RemoveListener(OnSwitchValueChanged);
            Switch.onTurnedOn.RemoveListener(OnSwitchValueChanged);
        }

        networkedToNetId.Remove(this);
        netIdtoNetworked.Remove(NetId);
        genericSwitchToNetId.Remove(Switch);
    }

    #region server
    public void Server_ReceiveSwitchState(bool isOn, ServerPlayer player)
    {
        if (!transform.PlayerCanReach(player))
        {
            Multiplayer.LogWarning($"Player \"{player.Username}\" tried to change switch [\"{Switch.persistenceKey}\", {NetId}] state but is too far away.");
            NetworkLifecycle.Instance.Server.SendGenericSwitchState(NetId, Switch.IsOn, player);
            return;
        }

        if (Switch.IsOn != isOn)
            Switch.IsOn = isOn;
    }

    #endregion
    public void Client_ReceiveSwitchState(bool isOn)
    {
        if (Switch.IsOn != isOn)
            Switch.IsOn = isOn;
    }
    #region client

    #endregion

    #region common
    private void GenerateNetId()
    {
        var hash = StringHashing.Fnv1aHash(Switch.persistenceKey);
        if(hash == 0 || hash == uint.MaxValue)
        {
            Multiplayer.LogError($"NetworkedGenericSwitch.GenerateNetId() generated invalid NetId for persistenceKey '{Switch.persistenceKey}'");
            return;
        }

        NetId = hash;

        if (netIdtoNetworked.ContainsKey(hash))
        {
            Multiplayer.LogError($"NetworkedGenericSwitch.GenerateNetId() generated duplicate NetId {hash} for persistenceKey '{Switch.persistenceKey}'");
            return;
        }

        netIdtoNetworked[hash] = this;
        networkedToNetId[this] = hash;
        genericSwitchToNetId[Switch] = hash;
    }

    private void OnSwitchValueChanged()
    {
        if (NetworkLifecycle.Instance.IsProcessingPacket)
            return;

        if (NetworkLifecycle.Instance.IsHost())
            NetworkLifecycle.Instance.Server.SendGenericSwitchState(NetId, Switch.IsOn);
        else
            NetworkLifecycle.Instance.Client.SendGenericSwitchState(NetId, Switch.IsOn);        
    }
    #endregion
}
