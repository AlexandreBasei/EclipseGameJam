using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandList : MonoBehaviour
{
    [SerializeField] private PanelRenderer uiPanel;
    public VisualElement uiRoot;
    public List<CommandSO> currentCommands;
    
    private int uiVersion;

    private void Awake()
    {
        uiPanel.RegisterUIReloadCallback(OnUIReload);
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
    }

}

