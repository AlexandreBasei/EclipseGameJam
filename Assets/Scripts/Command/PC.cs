using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

enum CurrentTab
{
    Mail,
    Garage
}

public class PC : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;

    private CurrentTab currentTab = CurrentTab.Mail;
    private VisualElement currentContent;
    private int uiVersion = -1;

    private void Awake()
    {
        panelRenderer.RegisterUIReloadCallback(OnUiReload);
    }

    private void OnDestroy()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUiReload);
    }

    private void OnUiReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        if (uiVersion == version) return;
        uiVersion = version;

        RegisterCallbacks(root);
    }

    private void RegisterCallbacks(VisualElement root)
    {
        root.Q<Button>("Mail").RegisterCallback<ClickEvent>(e => SwitchTab(root, CurrentTab.Mail));
        root.Q<Button>("Garage").RegisterCallback<ClickEvent>(e => SwitchTab(root, CurrentTab.Garage));
        root.Q<Button>("Cross").RegisterCallback<ClickEvent>(e => OnCrossClicked());
        root.Q<Button>("TruckUpgradeButton").RegisterCallback<ClickEvent>(e => OnUpgradeClicked(root, "TruckAmount"));
        root.Q<Button>("ChestUpgradeButton").RegisterCallback<ClickEvent>(e => OnUpgradeClicked(root, "ChestAmount"));
    }

    private void OnUpgradeClicked(VisualElement root, string upgradeName)
    {
        int requestedAmount = int.Parse(root.Q<Label>(upgradeName).text);
        if(DaysManager.Instance.tutoProgress == 8)
        {
            DaysManager.Instance.tutoFirstSleep();
        }
        
        if(PlayerHUD.Instance.moneyValue < requestedAmount) return;
        
        switch (upgradeName)
        {
            case "ChestAmount":
                DaysManager.Instance.UpgradeChest();
                root.Q<Label>("ChestAmount").text = DaysManager.Instance.chestLevel == DaysManager.Instance.maxChestLevel ? "Max" : $"{(DaysManager.Instance.chestLevel + 1) * 200}";
                break;
            case "TruckAmount":
                DaysManager.Instance.UpgradeTruck();
                root.Q<Label>("TruckAmount").text = DaysManager.Instance.truckLevel == DaysManager.Instance.maxTruckLevel ? "Max" :$"{(DaysManager.Instance.truckLevel + 1) * 200}";
                break;
        }
    }

    private void OnEnable()
    {
        Cursor.visible = true;
    }

    private void OnCrossClicked()
    {
        PlayerController.Instance.CanLook = true;
        PlayerController.Instance.CanMove = true;
        PlayerController.Instance.LockCursor();
        gameObject.SetActive(false);
    }

    private void SwitchTab(VisualElement root, CurrentTab newTab)
    {
        if(currentTab == newTab) return;
        
        currentTab = currentTab is CurrentTab.Mail ? CurrentTab.Garage : CurrentTab.Mail;
        
        DisplayStyle CommandDisplay = currentTab is CurrentTab.Mail ? DisplayStyle.Flex : DisplayStyle.None;
        DisplayStyle TruckDisplay = currentTab is CurrentTab.Mail ? DisplayStyle.None : DisplayStyle.Flex;
        
        root.Q<VisualElement>("MainView").hierarchy.ElementAt(1).style.display =
            new StyleEnum<DisplayStyle>(CommandDisplay);
        root.Q<VisualElement>("TruckView").style.display = new StyleEnum<DisplayStyle>(TruckDisplay);

        root.Q<Label>("UrlLabel").text =
            currentTab is CurrentTab.Mail ? "https://your-mail.fr" : "https://junk-car-garage.fr";
    }
}