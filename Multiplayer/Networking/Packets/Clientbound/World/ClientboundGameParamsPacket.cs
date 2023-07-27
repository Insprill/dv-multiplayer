using UnityEngine;

namespace Multiplayer.Networking.Packets.Clientbound.World;

public class ClientboundGameParamsPacket
{
    public string SerializedGameParams { get; set; }

    public void Apply(GameParams gameParams)
    {
        JsonUtility.FromJsonOverwrite(SerializedGameParams, gameParams);
    }

    public static ClientboundGameParamsPacket FromGameParams(GameParams gameParams)
    {
        return new ClientboundGameParamsPacket {
            SerializedGameParams = JsonUtility.ToJson(gameParams)
        };
    }
}
