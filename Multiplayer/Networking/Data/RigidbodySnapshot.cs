using LiteNetLib.Utils;
using Multiplayer.Networking.Serialization;
using UnityEngine;

namespace Multiplayer.Networking.Packets.Common;

public class RigidbodySnapshot
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }

    public static void Serialize(NetDataWriter writer, RigidbodySnapshot data)
    {
        Vector3Serializer.Serialize(writer, data.Position);
        Vector3Serializer.Serialize(writer, data.Rotation);
        Vector3Serializer.Serialize(writer, data.Velocity);
        Vector3Serializer.Serialize(writer, data.AngularVelocity);
    }

    public static RigidbodySnapshot Deserialize(NetDataReader reader)
    {
        return new RigidbodySnapshot {
            Position = Vector3Serializer.Deserialize(reader),
            Rotation = Vector3Serializer.Deserialize(reader),
            Velocity = Vector3Serializer.Deserialize(reader),
            AngularVelocity = Vector3Serializer.Deserialize(reader)
        };
    }

    public static RigidbodySnapshot From(Rigidbody rb)
    {
        return new RigidbodySnapshot {
            Position = rb.position,
            Rotation = rb.rotation.eulerAngles,
            Velocity = rb.velocity,
            AngularVelocity = rb.angularVelocity
        };
    }

    public void Apply(Rigidbody rb)
    {
        rb.MovePosition(Position);
        rb.MoveRotation(Quaternion.Euler(Rotation));
        rb.velocity = Velocity;
        rb.angularVelocity = AngularVelocity;
    }
}
