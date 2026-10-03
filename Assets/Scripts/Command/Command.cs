using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Command : MonoBehaviour
{
    [SerializeField] private PanelRenderer uiDocument;
    [SerializeField] private CommandSO[] commands;

    [SerializeField] private VisualTreeAsset commandComponent;
    [SerializeField] private VisualTreeAsset objectComponent;
    [SerializeField] private VisualTreeAsset tagComponent;
    
    private int uiVersion = 0;
    
    private void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }
    
    private void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }
    
    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        if (uiVersion == version)
            return;

        uiVersion = version;

        ListView listCommand = root.Q<ListView>("CommandList");

        foreach (var command in commands)
        { 
            if (command.accepted)
                return;
            //Create the command component
            VisualElement t_newCommand = commandComponent.Instantiate();
            t_newCommand.Q<Label>("ClientName").text = command.clientName;
            t_newCommand.Q<Label>("MoneyReward").text = $"{command.moneyReward}$";

            // Create the object component
            VisualElement t_newCommandObject = objectComponent.Instantiate();
            t_newCommandObject.Q<Label>("ObjectName").text = command.ObjectSO.objectName;
            
            // Generate the associated tags of the object
            GenerateTag(command.ObjectSO.tags, t_newCommandObject);
            
            //Insert the new object inside the command
            t_newCommand.hierarchy.Insert(1, t_newCommandObject);
            
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
        VisualElement t_newTagComponent = tagComponent.Instantiate();
        t_newTagComponent.Q<Label>("TagName").text = text;

        return t_newTagComponent;
    }
}

