using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandManager : PersistentSingleton<CommandManager>
{
    public List<CommandSO> CommandSos;
    
    [SerializeField] private CommandList AcceptedCommandUI;
    [SerializeField] private CommandList PCCommandList;
    
    public VisualTreeAsset commandComponent;
    public VisualTreeAsset objectComponent;
    public VisualTreeAsset tagComponent;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {   
            AcceptedCommandUI.gameObject.SetActive(!AcceptedCommandUI.gameObject.activeInHierarchy);
        }
    }

    public List<CommandSO> CurrentCommands()
    {
        List<CommandSO> filterList = new List<CommandSO>();

        foreach (var commandSo in CommandSos)
        {
            if (commandSo.state is CommandState.Accepted)
            {
                filterList.Add(commandSo);
            }
        }

        return filterList;
    }

    public List<CommandSO> NewCommands()
    {
        List<CommandSO> newCommands = new List<CommandSO>();


        return newCommands;
    }

    private void CreateCommandElement(CommandList uiDocument)
    {
        foreach (var command in CommandSos)
        { 
            ListView listCommand = uiDocument.uiRoot.Q<ListView>("CommandList");
            
            if (command.state is CommandState.Declined or CommandState.Finished)
                continue;
            
            //Create the command component
            VisualElement t_newCommand = commandComponent.Instantiate();
            t_newCommand.Q<Label>("ClientName").text = command.clientName;
            t_newCommand.Q<Label>("MoneyReward").text = $"{command.moneyReward}$";

            // Create the object component
            VisualElement t_newCommandObject = objectComponent.Instantiate();
            t_newCommandObject.Q<Label>("ObjectName").text = $"Object : {command.ObjectSO.objectName}";
            
            // Generate the associated tags of the object
            GenerateTag(command.ObjectSO.tags, t_newCommandObject);
            
            //Insert the new object inside the command
            t_newCommand.Q<VisualElement>("Main").hierarchy.Insert(1, t_newCommandObject);
            
            if (command.state == CommandState.Accepted)
            {
                t_newCommand.Q<VisualElement>("Main").AddToClassList("command_accepted");
                t_newCommand.Q<VisualElement>("ButtonContainer").RemoveFromHierarchy();
            }
            else
            {
                var buttons = t_newCommand.Q<VisualElement>("ButtonContainer");

                buttons.Q<Button>("AcceptButton").RegisterCallback<ClickEvent>(e =>
                {
                    command.state = CommandState.Accepted;
                    t_newCommand.Q<VisualElement>("Main").AddToClassList("command_accepted");
                    buttons.RemoveFromHierarchy();
                });

                buttons.Q<Button>("DeclineButton").RegisterCallback<ClickEvent>(e =>
                {
                    command.state = CommandState.Declined;
                    CommandSos.Remove(command);
                    t_newCommand.RemoveFromHierarchy();
                });
            }
            
            // Insert the new command into the list
            listCommand.hierarchy.Add(t_newCommand);
        }
    }
    
    private void GenerateTag(Tag objectTags, VisualElement objectContainer)
    {
        var query = Enum.GetValues(typeof(Tag))
            .Cast<Tag>()
            .Where(tag => tag != Tag.None && objectTags.HasFlag(tag)).ToList();

        if (query.Count < 2)
            objectContainer.Q<VisualElement>("TagContainer").hierarchy.Add(CreateTagElement("None"));
        else
            foreach (Tag tag in query) 
                objectContainer.Q<VisualElement>("TagContainer").hierarchy.Add(CreateTagElement($"{tag}"));
    }
    
    private VisualElement CreateTagElement(string text)
    {
        VisualElement t_newTagComponent = CommandManager.Instance.tagComponent.Instantiate();
        t_newTagComponent.Q<Label>("TagName").text = text;

        return t_newTagComponent;
    }
}
