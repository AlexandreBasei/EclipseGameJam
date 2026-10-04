using UnityEngine;
using UnityEngine.UIElements;

enum CurrentTab
{
    Mail,
    Gagare
}

public class PC : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset commandListUI;
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private CommandList pcCommandList; 

    private CurrentTab currentTab = CurrentTab.Mail;
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

        VisualElement newCommandList = commandListUI.Instantiate();
        root.Q<VisualElement>("MainView").hierarchy.Add(newCommandList);
        
    }

    private void RegisterCallbacks(VisualElement root)
    {
        root.Q<Button>("Mail").RegisterCallback<ClickEvent>(e => OnMailClicked());
        root.Q<Button>("Garage").RegisterCallback<ClickEvent>(e => OnCarClicked());
    }

    private void OnMailClicked()
    {
        if (currentTab is CurrentTab.Mail) return;

        currentTab = CurrentTab.Gagare;
    }

    private void OnCarClicked()
    {
        if (currentTab is CurrentTab.Gagare) return;

        currentTab = CurrentTab.Mail;
    }
}