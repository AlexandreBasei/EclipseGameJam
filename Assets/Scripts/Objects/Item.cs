using UnityEngine;
using TMPro;

public class Item : MonoBehaviour, IInteractable
{
    public ObjectSO itemData;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    void Start()
    {
        if (_itemNameText != null && itemData != null)
        {
            _itemNameText.text = itemData.objectName;
        }

        GetComponent<Outline>().OutlineColor = _outlineColor;
    }

    public void SetItemNameVisible(bool visible)
    {
        if (_itemNameText != null)
            _itemNameText.gameObject.SetActive(visible);
    }

    public void PickUp()
    {
        if (DaysManager.Instance.IsInWorkshop)
        {
            // WIP : prendre l'objet
        }
        else
        {
            // WIP : envoyer vers la voiture
            Destroy(gameObject);
        }
    }

    public void Rotate()
    {
        // WIP
    }

    public void Snap()
    {
        // WIP
    }
}
