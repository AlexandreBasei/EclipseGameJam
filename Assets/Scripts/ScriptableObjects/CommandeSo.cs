using UnityEngine;

[System.Serializable]
public struct CommandObject
{
    public ObjectSO objectSo;
    public bool required;
}

[CreateAssetMenu(fileName = "CommandSO", menuName = "Scriptable Objects/CommandsSO")]
public class CommandSo : ScriptableObject
{
    public string clientName;
    public int moneyReward;
    public CommandObject[] listObjects;
    public bool accepted;
}