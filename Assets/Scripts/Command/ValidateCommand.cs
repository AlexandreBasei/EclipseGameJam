using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ValidateCommand : MonoBehaviour
{
    [SerializeField] private PanelRenderer commandUi;
    [SerializeField] private SellManager sellManager;

    private VisualElement rootUI;

    private void Start()
    {
        commandUi.RegisterUIReloadCallback(OnUIReload);
        CommandManager.Instance.CommandsChanged += RefreshShipButtons;
    }

    private void OnDestroy()
    {
        commandUi.UnregisterUIReloadCallback(OnUIReload);

        if (CommandManager.Instance != null)
            CommandManager.Instance.CommandsChanged -= RefreshShipButtons;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        rootUI = root;
        rootUI.Q<Button>("CrossButton").RegisterCallback<ClickEvent>(OnCrossButtonClicked);

        // Attend que la liste des commandes ait fini d'être reconstruite.
        rootUI.schedule.Execute(SetupShipButton);
    }

    private void RefreshShipButtons()
    {
        if (rootUI != null)
            rootUI.schedule.Execute(SetupShipButton);
    }

    private void OnCrossButtonClicked(ClickEvent e)
    {
        PlayerController.Instance.CanLook = true;
        PlayerController.Instance.CanMove = true;
        PlayerController.Instance.LockCursor();
        PlayerInteract.Instance.isInComputer = false;
        gameObject.SetActive(false);
    }

    private void SetupShipButton()
    {
        if (rootUI == null)
            return;

        var commands = CommandManager.Instance.GetList(ListType.Current);
        var commandListUI = rootUI.Q<VisualElement>("CommandList").Children().ToList();

        foreach (var button in rootUI.Query<Button>("ShipButton").ToList())
            button.RemoveFromHierarchy();

        int count = Mathf.Min(commands.Count, commandListUI.Count);

        for (int i = 0; i < count; i++)
        {
            var command = commands[i];
            var shipButton = new Button { text = "Ship", name = "ShipButton" };
            shipButton.AddToClassList("shipButton");
            shipButton.style.display = DisplayStyle.Flex;

            commandListUI[i].Q<VisualElement>("Main").hierarchy.Add(shipButton);
            shipButton.RegisterCallback<ClickEvent>(_ => ShipProduct(command));
        }
    }

    private void ShipProduct(CommandSO command)
    {
        var objectTags = sellManager.tagsInSellZone;

        if (objectTags.Length < 1 || !sellManager.canSell)
            return;

        int finalReward = command.moneyReward;

        List<Tag> commandTags = Enum.GetValues(typeof(Tag)).Cast<Tag>()
            .Where(t => t != Tag.None && command.ObjectSO.tags.HasFlag(t))
            .ToList();

        foreach (var tag in commandTags)
        {
            if (!objectTags.Contains(tag))
                finalReward /= 2;
        }

        if (DaysManager.Instance.tutoProgress == 6)
            DaysManager.Instance.tutoFirstChestUse();

        MoneyUI.Instance.UpdateMoney(finalReward);
        CommandManager.Instance.ShipCommand(command);
        sellManager.SellCurrentAssembly();
    }
}