using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class CommandListView : IDisposable
{
    private readonly ScrollView list;
    private readonly CommandUIAssets assets;
    private readonly CommandManager manager;
    private readonly ListType listType;

    public CommandListView(ScrollView list, CommandUIAssets assets, CommandManager manager, ListType listType)
    {
        this.list = list;
        this.assets = assets;
        this.manager = manager;
        this.listType = listType;

        manager.CommandsChanged += Rebuild;
        Rebuild();
    }

    public void Dispose()
    {
        manager.CommandsChanged -= Rebuild;
    }

    private void Rebuild()
    {
        list.Clear();

        foreach (CommandSO command in manager.GetList(listType))
        {
            if (command.state is CommandState.Declined or CommandState.Finished)
                continue;

            list.Add(CreateItem(command));
        }
    }

    private VisualElement CreateItem(CommandSO command)
    {
        VisualElement item = assets.commandComponent.Instantiate();
        item.Q<Label>("ClientName").text = command.clientName;
        item.Q<Label>("MoneyReward").text = $"{command.moneyReward}$";

        VisualElement obj = assets.objectComponent.Instantiate();
        obj.Q<Label>("ObjectName").text = $"Object : {command.ObjectSO.objectName}";
        FillTags(command.ObjectSO.tags, obj.Q<VisualElement>("TagContainer"));

        VisualElement main = item.Q<VisualElement>("Main");
        main.hierarchy.Insert(1, obj);

        VisualElement buttons = item.Q<VisualElement>("ButtonContainer");

        if (command.state is CommandState.Accepted)
        {
            main.AddToClassList("command_accepted");
            buttons.RemoveFromHierarchy();
        }
        else
        {
            buttons.Q<Button>("AcceptButton").clicked += () => manager.AcceptCommand(command);
            if (DaysManager.Instance.currentDay == 1)
                buttons.Remove(buttons.Q<Button>("DeclineButton"));
            else
                buttons.Q<Button>("DeclineButton").clicked += () => manager.DeclineCommand(command);
        }

        return item;
    }

    private void FillTags(Tag objectTags, VisualElement container)
    {
        List<Tag> tags = Enum.GetValues(typeof(Tag)).Cast<Tag>()
            .Where(t => t != Tag.None && objectTags.HasFlag(t))
            .ToList();

        if (tags.Count == 0)
        {
            container.Add(CreateTag("None"));
            return;
        }

        foreach (Tag tag in tags)
            container.Add(CreateTag(tag.ToString()));
    }

    private VisualElement CreateTag(string text)
    {
        VisualElement tag = assets.tagComponent.Instantiate();
        tag.Q<Label>("TagName").text = text;
        return tag;
    }
}