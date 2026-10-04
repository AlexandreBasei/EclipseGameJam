using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class CommandManager : PersistentSingleton<CommandManager>
{
    [SerializeField] private List<CommandSO> allCommands;

    [SerializeField] private CommandList AcceptedCommandUI;
    [SerializeField] private CommandList PCCommandList;

    public VisualTreeAsset commandComponent;
    public VisualTreeAsset objectComponent;
    public VisualTreeAsset tagComponent;

    private List<CommandSO> newCommands;
    private List<CommandSO> currentCommands;
    private bool initialized;
    
    public event Action CommandsChanged;
    
    public List<CommandSO> NewCommands
    {
        get { Init(); return newCommands; }
    }

    public List<CommandSO> CurrentCommands
    {
        get { Init(); return currentCommands; }
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            AcceptedCommandUI.gameObject.SetActive(!AcceptedCommandUI.gameObject.activeSelf);
        }
    }

    private void Init()
    {
        if (initialized)
            return;

        initialized = true;

        currentCommands = allCommands.Where(c => c.state is CommandState.Accepted).ToList();
        newCommands = GetNewCommands();
    }

    private List<CommandSO> GetNewCommands()
    {
        List<CommandSO> available = allCommands
            .Where(c => c.state is not (CommandState.Accepted or CommandState.Declined or CommandState.Finished))
            .ToList();

        List<CommandSO> result = new List<CommandSO>();
        int count = Mathf.Min(DaysManager.Instance.currentDay, available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            result.Add(available[index]);
            available.RemoveAt(index); 
        }

        return result;
    }

    public void CreateCommandElement(CommandList uiDocument, List<CommandSO> commandList)
    {
        if (uiDocument.uiRoot == null)
            return;

        ScrollView listCommand = uiDocument.uiRoot.Q<ScrollView>("CommandList");
        if (listCommand == null)
            return;

        listCommand.Clear(); 

        foreach (var command in commandList)
        {
            if (command.state is CommandState.Declined or CommandState.Finished)
                continue;

            // Command component
            VisualElement t_newCommand = commandComponent.Instantiate();
            t_newCommand.Q<Label>("ClientName").text = command.clientName;
            t_newCommand.Q<Label>("MoneyReward").text = $"{command.moneyReward}$";

            // Object component
            VisualElement t_newCommandObject = objectComponent.Instantiate();
            t_newCommandObject.Q<Label>("ObjectName").text = $"Object : {command.ObjectSO.objectName}";

            GenerateTag(command.ObjectSO.tags, t_newCommandObject);

            // Insère l'objet dans la commande
            t_newCommand.Q<VisualElement>("Main").hierarchy.Insert(1, t_newCommandObject);

            if (command.state == CommandState.Accepted)
            {
                t_newCommand.Q<VisualElement>("Main").AddToClassList("command_accepted");
                t_newCommand.Q<VisualElement>("ButtonContainer").RemoveFromHierarchy();
            }
            else
            {
                var buttons = t_newCommand.Q<VisualElement>("ButtonContainer");

                buttons.Q<Button>("AcceptButton").RegisterCallback<ClickEvent>(e => AcceptCommand(command));
                buttons.Q<Button>("DeclineButton").RegisterCallback<ClickEvent>(e => DeclineCommand(command));
            }

            listCommand.Add(t_newCommand);   
        }
    }
    
    private void AcceptCommand(CommandSO command)
    {
        command.state = CommandState.Accepted;
        newCommands.Remove(command);

        if (!currentCommands.Contains(command))
            currentCommands.Add(command);

        CommandsChanged?.Invoke();
    }

    private void DeclineCommand(CommandSO command)
    {
        command.state = CommandState.Declined;
        newCommands.Remove(command);
        currentCommands.Remove(command);

        CommandsChanged?.Invoke();
    }

    private void GenerateTag(Tag objectTags, VisualElement objectContainer)
    {
        var tags = Enum.GetValues(typeof(Tag))
            .Cast<Tag>()
            .Where(tag => tag != Tag.None && objectTags.HasFlag(tag))
            .ToList();

        var container = objectContainer.Q<VisualElement>("TagContainer");

        if (tags.Count < 2)
            container.hierarchy.Add(CreateTagElement("None"));
        else
            foreach (Tag tag in tags)
                container.hierarchy.Add(CreateTagElement(tag.ToString()));
    }

    private VisualElement CreateTagElement(string text)
    {
        VisualElement t_newTagComponent = tagComponent.Instantiate();
        t_newTagComponent.Q<Label>("TagName").text = text;
        return t_newTagComponent;
    }
}