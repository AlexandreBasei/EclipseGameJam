using UnityEngine;

[CreateAssetMenu(fileName = "CommandSO", menuName = "Scriptable Objects/CommandsSO")]
public class CommandSO : ScriptableObject
{
    public string clientName;
    public int moneyReward;
    public ObjectSO ObjectSO;
    public bool accepted;
}