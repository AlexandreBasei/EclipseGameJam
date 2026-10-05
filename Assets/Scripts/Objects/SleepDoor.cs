using TMPro;
using UnityEngine;
using TMPro;

public class SleepDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    public TextMeshProUGUI ItemNameText => _itemNameText;

    public void PickUp(Camera playerCamera = null)
    {
        if (DaysManager.Instance.tutoFinished == false)
        {
            return;
        }
        FadeInOut.Instance.FadeIn();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.coqSound);
        DaysManager.Instance.NextDay();
        FadeInOut.Instance.FadeOut();
    }

    public void SetItemNameVisible(bool visible)
    {
        if (ItemNameText != null)
        {
            ItemNameText.text = DaysManager.Instance.rentPrice.ToString() + " $";
            print(visible);
            ItemNameText.gameObject.SetActive(visible);
        }

    }
}
