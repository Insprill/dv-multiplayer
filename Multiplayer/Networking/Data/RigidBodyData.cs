using LiteNetLib.Utils;
using Multiplayer.Networking.Serialization;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Common;

public struct RigidBodyData
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }

    public void Apply(Rigidbody rb)
    {
        rb.MovePosition(Position);
        rb.MoveRotation(Quaternion.Euler(Rotation));
        rb.velocity = Velocity;
        rb.angularVelocity = AngularVelocity;
    }

    public static RigidBodyData From(Rigidbody rb)
    {
        return new RigidBodyData {
            Position = rb.position,
            Rotation = rb.rotation.eulerAngles,
            Velocity = rb.velocity,
            AngularVelocity = rb.angularVelocity
        };
    }

    public static void Serialize(NetDataWriter writer, RigidBodyData data)
    {
        Vector3Serializer.Serialize(writer, data.Position);
        Vector3Serializer.Serialize(writer, data.Rotation);
        Vector3Serializer.Serialize(writer, data.Velocity);
        Vector3Serializer.Serialize(writer, data.AngularVelocity);
    }

    public static RigidBodyData Deserialize(NetDataReader reader)
    {
        return new RigidBodyData {
            Position = Vector3Serializer.Deserialize(reader),
            Rotation = Vector3Serializer.Deserialize(reader),
            Velocity = Vector3Serializer.Deserialize(reader),
            AngularVelocity = Vector3Serializer.Deserialize(reader)
        };
    }
}
