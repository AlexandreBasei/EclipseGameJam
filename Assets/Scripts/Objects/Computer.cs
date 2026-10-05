using UnityEngine;

public class Computer : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;

    public void PickUp(Camera playerCamera = null)
    {
        CommandManager.Instance.computerUI.SetActive(true);
        PlayerController.Instance.CanLook = false;
        PlayerController.Instance.CanMove = false;
        PlayerController.Instance.UnlockCursor();
    }

    public void SetItemNameVisible(bool visible)
    {
        return;
    }
}
