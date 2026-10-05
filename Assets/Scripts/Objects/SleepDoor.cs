using UnityEngine;

public class SleepDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;

    public void PickUp(Camera playerCamera = null)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.coqSound);
        DaysManager.Instance.NextDay();
    }

    public void SetItemNameVisible(bool visible)
    {
        return;
    }
}
