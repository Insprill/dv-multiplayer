using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Multiplayer.Networking.Packets.Common;
using Multiplayer.Networking.Serialization;

namespace Multiplayer.Networking.Listeners;

public abstract class NetworkManager : INetEventListener
{
    protected readonly NetPacketProcessor netPacketProcessor;
    protected readonly NetManager netManager;
    protected readonly NetDataWriter cachedWriter = new();

    protected abstract string LogPrefix { get; }

    public bool IsRunning => netManager.IsRunning;
    public bool IsProcessingPacket { get; private set; }

    protected NetworkManager(Settings settings)
    {
        netManager = new NetManager(this);
        netPacketProcessor = new NetPacketProcessor();
        RegisterNestedTypes();
        OnSettingsUpdated(settings);
        Settings.OnSettingsUpdated += OnSettingsUpdated;
        // ReSharper disable once VirtualMemberCallInConstructor
        Subscribe();
    }

    private void RegisterNestedTypes()
    {
        netPacketProcessor.RegisterNestedType(ModInfo.Serialize, ModInfo.Deserialize);
        netPacketProcessor.RegisterNestedType(BogieData.Serialize, BogieData.Deserialize);
        netPacketProcessor.RegisterNestedType(Vector2Serializer.Serialize, Vector2Serializer.Deserialize);
        netPacketProcessor.RegisterNestedType(Vector3Serializer.Serialize, Vector3Serializer.Deserialize);
    }

    private void OnSettingsUpdated(Settings settings)
    {
        if (netManager == null)
            return;
        netManager.NatPunchEnabled = settings.EnableNatPunch;
        netManager.AutoRecycle = settings.ReuseNetPacketReaders;
        netManager.UseNativeSockets = settings.UseNativeSockets;
    }

    public void PollEvents()
    {
        netManager.PollEvents();
    }

    public void Stop()
    {
        netManager.Stop(true);
        Settings.OnSettingsUpdated -= OnSettingsUpdated;
    }

    protected NetDataWriter WritePacket<T>(T packet) where T : class, new()
    {
        cachedWriter.Reset();
        netPacketProcessor.Write(cachedWriter, packet);
        return cachedWriter;
    }

    protected void SendPacket<T>(NetPeer peer, T packet, DeliveryMethod deliveryMethod) where T : class, new()
    {
        peer?.Send(WritePacket(packet), deliveryMethod);
    }

    protected abstract void Subscribe();

    #region Events

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            IsProcessingPacket = true;
            netPacketProcessor.ReadAllPackets(reader, peer);
        }
        catch (ParseException e)
        {
            Multiplayer.LogWarning($"Failed to parse packet: {e.Message}");
        }
        finally
        {
            IsProcessingPacket = false;
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Multiplayer.LogError($"Network error from {endPoint}: {socketError}");
    }

    public abstract void OnPeerConnected(NetPeer peer);
    public abstract void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
    public abstract void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType);
    public abstract void OnNetworkLatencyUpdate(NetPeer peer, int latency);
    public abstract void OnConnectionRequest(ConnectionRequest request);

    #endregion

    #region Logging

    public void Log(object msg)
    {
        Multiplayer.Log($"{LogPrefix} {msg}");
    }

    public void LogWarning(object msg)
    {
        Multiplayer.LogWarning($"{LogPrefix} {msg}");
    }

    public void LogError(object msg)
    {
        Multiplayer.LogError($"{LogPrefix} {msg}");
    }

    #endregion
}
