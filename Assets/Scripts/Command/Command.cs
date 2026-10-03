using System;
using UnityEngine;
using UnityEngine.UIElements;

public class Command : MonoBehaviour
{
    [SerializeField] private PanelRenderer uiDocument;
    [SerializeField] private CommandSO command;
    
    private int uiVersion = 0;


    private void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }
    
    private void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }
    
    private void GenerateTag()
    {
        UIDocument tagContainer = uiDocument.GetComponent<UIDocument>();
        Debug.Log(tagContainer);
    }
    
    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
    {
        if (uiVersion == version)
            return;

        uiVersion = version;
    }
}

