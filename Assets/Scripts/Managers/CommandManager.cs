using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ListType
{
    New,
    Current
}

public class CommandManager : PersistentSingleton<CommandManager>
{
    [SerializeField] private List<CommandSO> allCommands;

    private List<CommandSO> newCommands;
    public List<CommandSO> currentCommands;
    private bool initialized;
    public GameObject computerUI;

    public event Action CommandsChanged;

    public IReadOnlyList<CommandSO> GetList(ListType type)
    {
        Init();
        return type is ListType.New ? newCommands : currentCommands;
    }

    private void Start() => Init();

    private void Init()
    {
        if (initialized) return;
        initialized = true;
        currentCommands ??= new List<CommandSO>();
        ResetAllCommand();
    }

    public List<CommandSO> PickNewCommands()
    {
        var available = allCommands
            .Distinct()
            .Where(c => c.state is not (CommandState.Accepted or CommandState.Declined or CommandState.Finished))
            .Where(c => c.ObjectSO.tags.Count() <= DaysManager.Instance.currentDay + 1)
            .ToList();

        var result = new List<CommandSO>();
        int count = Mathf.Min(DaysManager.Instance.currentDay, available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            result.Add(available[index]);
            available.RemoveAt(index);
        }

        return result;
    }

    public void SetNewCommands()
    {
        DiscardAllCurrentsCommands();
        newCommands = PickNewCommands();
        CommandsChanged?.Invoke();
    }

    public void DiscardAllCurrentsCommands()
    {
        currentCommands.Clear();
    }

    public void AcceptCommand(CommandSO command)
    {
        AudioManager.Instance.PlayClic();
        command.state = CommandState.Accepted;
        newCommands.Remove(command);

        if (!currentCommands.Contains(command) && currentCommands.Count < 3)
            currentCommands.Add(command);

        CommandsChanged?.Invoke();
    }

    public void ShipCommand(CommandSO command)
    {
        if (!currentCommands.Contains(command)) return;
        command.state = CommandState.Finished;
        currentCommands.Remove(command);
        
        CommandsChanged?.Invoke();

    }

    public void DeclineCommand(CommandSO command)
    {
        AudioManager.Instance.PlayClic();
        command.state = CommandState.Declined;
        newCommands.Remove(command);
        currentCommands.Remove(command);

        CommandsChanged?.Invoke();
    }

    public void ResetAllCommand()
    {
        currentCommands ??= new List<CommandSO>();
        currentCommands.Clear();
        newCommands = new List<CommandSO>();
        foreach (var command in allCommands)
        {
            command.state = CommandState.Idle;
        }

        newCommands = PickNewCommands();
        CommandsChanged?.Invoke();
    }
}