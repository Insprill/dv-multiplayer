using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Multiplayer.Utils;

public static class UnityExtensions
{
    [NotNull]
    public static GameObject NewChild(this GameObject parent, string name)
    {
        return new GameObject(name) {
            transform = {
                parent = parent.transform
            }
        };
    }

    [CanBeNull]
    public static GameObject FindChildByName(this Component parent, string name)
    {
        return parent.gameObject.FindChildByName(name);
    }

    [CanBeNull]
    public static GameObject FindChildByName(this GameObject parent, string name)
    {
        Transform child = parent.transform.FindChildByName(name);
        return child == null ? null : child.gameObject;
    }

    [CanBeNull]
    public static Transform FindChildByName(this Transform parent, string name)
    {
        return FindChildrenByName(parent, name).FirstOrDefault();
    }

    [NotNull]
    public static List<GameObject> FindChildrenByName(this Component parent, string name)
    {
        return FindChildrenByName(parent.gameObject, name);
    }

    [NotNull]
    public static List<GameObject> FindChildrenByName(this GameObject parent, string name)
    {
        List<Transform> transforms = FindChildrenByName(parent.transform, name);
        List<GameObject> gameObjects = new(transforms.Count);
        foreach (Transform t in transforms)
            gameObjects.Add(t.gameObject);
        return gameObjects;
    }

    [NotNull]
    public static List<Transform> FindChildrenByName(this Transform parent, string name)
    {
        List<Transform> list = new();
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name)
                list.Add(t);

        return list;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    public static T GetOrAddComponent<T>(this Component component) where T : Component
    {
        return component.gameObject.GetOrAddComponent<T>();
    }
}
