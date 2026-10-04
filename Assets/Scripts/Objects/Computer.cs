using UnityEngine;

public class Computer : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;

    public void PickUp(Camera playerCamera = null)
    {
        // OUVRIR UI MAGASIN ICI
        return;
    }

    public void SetItemNameVisible(bool visible)
    {
        return;
    }
}
