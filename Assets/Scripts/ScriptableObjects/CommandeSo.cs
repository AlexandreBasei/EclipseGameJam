using UnityEngine;


public enum CommandState
{
    Idle,
    Accepted,
    Declined,
    Finished
}

[CreateAssetMenu(fileName = "CommandSO", menuName = "Scriptable Objects/CommandsSO")]
public class CommandSO : ScriptableObject
{
    public string clientName;
    public int moneyReward;
    public ObjectSO ObjectSO;
    public CommandState state;

}