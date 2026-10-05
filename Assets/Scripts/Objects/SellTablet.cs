using UnityEngine;
using TMPro;

public class SellTablet : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    [SerializeField] private GameObject tabletCommand;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    public TextMeshProUGUI ItemNameText => null;

    public void PickUp(Camera playerCamera = null)
    {
        PlayerInteract.Instance.isInComputer = true;
        PlayerController.Instance.CanLook = false;
        PlayerController.Instance.CanMove = false;
        PlayerController.Instance.UnlockCursor();
        tabletCommand.SetActive(true);
    }

    public void SetItemNameVisible(bool visible)
    {
        return;
    }
}
