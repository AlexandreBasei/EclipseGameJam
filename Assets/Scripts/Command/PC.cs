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

    private VisualElement uiRoot;
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
        uiRoot = root;

        RegisterCallbacks();
        UpdateMoneyUI();
    }

    private void RegisterCallbacks()
    {
        uiRoot.Q<Button>("Mail").RegisterCallback<ClickEvent>(e => SwitchTab(CurrentTab.Mail));
        uiRoot.Q<Button>("Garage").RegisterCallback<ClickEvent>(e => SwitchTab(CurrentTab.Garage));
        uiRoot.Q<Button>("Cross").RegisterCallback<ClickEvent>(e => OnCrossClicked());
        uiRoot.Q<Button>("TruckUpgradeButton").RegisterCallback<ClickEvent>(e => OnUpgradeClicked("TruckAmount"));
        uiRoot.Q<Button>("ChestUpgradeButton").RegisterCallback<ClickEvent>(e => OnUpgradeClicked("ChestAmount"));
    }

    private void OnUpgradeClicked(string upgradeName)
    {
        AudioManager.Instance.PlayClic();
        int requestedAmount = int.Parse(uiRoot.Q<Label>(upgradeName).text);
        if(DaysManager.Instance.tutoProgress == 8)
        {
            DaysManager.Instance.tutoFirstSleep();
        }

        int playerMoney = PlayerHUD.Instance.moneyValue;
        
        if(playerMoney < requestedAmount) return;
        
        switch (upgradeName)
        {
            case "ChestAmount":
                DaysManager.Instance.UpgradeChest();
                uiRoot.Q<Label>("ChestAmount").text = DaysManager.Instance.chestLevel == DaysManager.Instance.maxChestLevel ? "Max" : $"{(DaysManager.Instance.chestLevel + 1) * 200}";
                break;
            case "TruckAmount":
                DaysManager.Instance.UpgradeTruck();
                uiRoot.Q<Label>("TruckAmount").text = DaysManager.Instance.truckLevel == DaysManager.Instance.maxTruckLevel ? "Max" :$"{(DaysManager.Instance.truckLevel + 1) * 200}";
                break;
        }
        PlayerHUD.Instance.ChangedMoneyValue(playerMoney - requestedAmount);
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        uiRoot.Q<Label>("CurrentMoney").text = $"{PlayerHUD.Instance.moneyValue}$";
    }

    private void OnEnable()
    {
        Cursor.visible = true;
    }

    private void OnCrossClicked()
    {
        AudioManager.Instance.PlayClic();
        PlayerController.Instance.CanLook = true;
        PlayerController.Instance.CanMove = true;
        PlayerController.Instance.LockCursor();
        gameObject.SetActive(false);
        if (DaysManager.Instance.tutoStarted && DaysManager.Instance.tutoProgress == 1)
        {
            DaysManager.Instance.tutoFirstReview();
        }
    }

    private void SwitchTab(CurrentTab newTab)
    {
        AudioManager.Instance.PlayClic();
        if(currentTab == newTab) return;
        
        currentTab = currentTab is CurrentTab.Mail ? CurrentTab.Garage : CurrentTab.Mail;
        
        DisplayStyle CommandDisplay = currentTab is CurrentTab.Mail ? DisplayStyle.Flex : DisplayStyle.None;
        DisplayStyle TruckDisplay = currentTab is CurrentTab.Mail ? DisplayStyle.None : DisplayStyle.Flex;
        
        uiRoot.Q<VisualElement>("MainView").hierarchy.ElementAt(1).style.display =
            new StyleEnum<DisplayStyle>(CommandDisplay);
        uiRoot.Q<VisualElement>("TruckView").style.display = new StyleEnum<DisplayStyle>(TruckDisplay);

        uiRoot.Q<Label>("UrlLabel").text =
            currentTab is CurrentTab.Mail ? "https://your-mail.fr" : "https://junk-car-garage.fr";
    }
}