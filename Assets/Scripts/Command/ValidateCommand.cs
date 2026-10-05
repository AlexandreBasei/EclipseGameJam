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
    private IReadOnlyList<CommandSO> commandList;

    private void Start()
    {
        commandUi.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        commandUi.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        commandList = CommandManager.Instance.GetList(ListType.Current);
        rootUI = root;
        SetupShipButton();
        root.Q<Button>("CrossButton").RegisterCallback<ClickEvent>(OnCrossButtonClicked);
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
        List<VisualElement> commandListUI = rootUI.Q<VisualElement>("CommandList").Children().ToList();

        for (int i = 0; i < commandList.Count; i++)
        {
            var shipButton = rootUI.Q<Button>("ShipButton");
            shipButton.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            var command = commandList[i];
            shipButton.RegisterCallback<ClickEvent>(e => ShipProduct(command));
        }
    }

    private void ShipProduct(CommandSO command)
    {
        var objectTags = sellManager.tagsInSellZone;

        if (objectTags.Length < 1 || !sellManager.canSell) return;

        int finalReward = command.moneyReward;

        List<Tag> commandTags = Enum.GetValues(typeof(Tag)).Cast<Tag>()
            .Where(t => t != Tag.None && command.ObjectSO.tags.HasFlag(t))
            .ToList();

        foreach (var tag in commandTags)
        {
            if (!objectTags.Contains(tag))
                finalReward /= 2;
        }

        if(DaysManager.Instance.tutoProgress == 6)
            DaysManager.Instance.tutoFirstChestUse();
        MoneyUI.Instance.UpdateMoney(finalReward);
        CommandManager.Instance.ShipCommand(command);
        sellManager.SellCurrentAssembly();
        SetupShipButton();
    }
}
