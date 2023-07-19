using LiteNetLib.Utils;
using UnityEngine;

namespace Multiplayer.Networking.Serialization;

public static class Vector2Serializer
{
    public static void Serialize(NetDataWriter writer, Vector2 vec)
    {
        writer.Put(vec.x);
        writer.Put(vec.y);
    }

    public static Vector2 Deserialize(NetDataReader reader)
    {
        return new Vector2(reader.GetFloat(), reader.GetFloat());
    }
}
