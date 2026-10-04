using UnityEngine;
using UnityEngine.UIElements;

public class CommandList : MonoBehaviour
{
    [SerializeField] private PanelRenderer panel;
    [SerializeField] private ListType listType;
    [SerializeField] private CommandUIAssets assets;

    [Header("Optionnel : layout à injecter dans le panel (ex: le PC)")]
    [SerializeField] private VisualTreeAsset layout;
    [SerializeField] private string layoutParent = "MainView";

    private CommandListView view;
    private int uiVersion = -1;

    private void Awake()
    {
        panel.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        panel.UnregisterUIReloadCallback(OnUIReload);
        view?.Dispose();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if (version == uiVersion) return;
        uiVersion = version;

        view?.Dispose();
        
        if (layout != null)
            root.Q<VisualElement>(layoutParent).Add(layout.Instantiate());

        var scroll = root.Q<ScrollView>("CommandList");
        if (scroll == null)
        {
            Debug.LogError($"ScrollView 'CommandList' introuvable dans {name}", this);
            return;
        }

        view = new CommandListView(scroll, assets, CommandManager.Instance, listType);
    }
}