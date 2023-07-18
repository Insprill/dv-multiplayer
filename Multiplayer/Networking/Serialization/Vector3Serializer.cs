using LiteNetLib.Utils;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Common;

public static class Vector3Serializer
{
    public static void Serialize(NetDataWriter writer, Vector3 vec)
    {
        writer.Put(vec.x);
        writer.Put(vec.y);
        writer.Put(vec.z);
    }

    public static Vector3 Deserialize(NetDataReader reader)
    {
        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}
