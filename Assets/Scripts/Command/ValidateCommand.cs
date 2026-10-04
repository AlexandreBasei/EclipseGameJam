using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ValidateCommand : MonoBehaviour
{
    [SerializeField] private PanelRenderer commandUi;

    private void Start()
    {
        commandUi.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        SetupShipButton(root.Q<VisualElement>("CommandList").Children().ToList());
    }

    private void SetupShipButton(List<VisualElement> commandList)
    {
        foreach (var command in commandList)
        {
            
            Button shipButton = new Button();
            shipButton.text = "Ship";
            shipButton.AddToClassList("shipButton");
            
            command.Q<VisualElement>("Main").hierarchy.Add(shipButton);
            shipButton.RegisterCallback<ClickEvent>(e => ShipProduct());
        } 
    }

    private void ShipProduct()
    {
        //TODO
    }
}
