using NUnit.Framework;
using UnityEngine;


[System.Flags]
public enum Tag
{
    None = 0,
    Wheel = 1 << 0,
    Engine = 1 << 1,
    Heavy = 1 << 2,
    Luminous = 1 << 3,
    Expensive = 1 << 4,
    Wooden = 1 << 5,
    Metal = 1 << 6,
    Plastic = 1 << 7,
    Hot = 1 << 8,
    Cold = 1 << 9,
    Seatable = 1 << 10,
    Organic = 1 << 11,
    Musical = 1 << 12,
    Handheld = 1 << 13,
    Cosy = 1 << 14,
    Windy = 1 << 15,
    Sharp = 1 << 16,
    Bouncy = 1 << 17,
    Container = 1 << 18,

}

[CreateAssetMenu(fileName = "ObjectSO", menuName = "Scriptable Objects/ObjectSO")]
public class ObjectSO : ScriptableObject
{
    public string objectName;
    public Tag tags;
}

