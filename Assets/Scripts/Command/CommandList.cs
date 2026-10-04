using UnityEngine;
using UnityEngine.UIElements;

public enum ListType
{
    New,
    Current
}

public class CommandList : MonoBehaviour
{
    [SerializeField] private PanelRenderer uiPanel;
    [SerializeField] private ListType listType;
    public VisualElement uiRoot;

    private CommandManager manager;
    private int uiVersion = -1;

    private void Awake()
    {
        uiPanel.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnEnable()
    {
        manager = CommandManager.Instance;
        manager.CommandsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.CommandsChanged -= Refresh;
    }

    private void OnDestroy()
    {
        uiPanel.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        if (uiVersion == version)
            return;

        uiVersion = version;
        uiRoot = root;

        Refresh();
    }

    private void Refresh()
    {
        if (uiRoot == null || manager == null)
            return;

        manager.CreateCommandElement(this, listType is ListType.New ? manager.NewCommands : manager.CurrentCommands);
    }
}