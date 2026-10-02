using UnityEngine;
using TMPro;

public class Item : MonoBehaviour, IInteractable
{
    public ObjectSO itemData;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    void Start()
    {
        if (_itemNameText != null && itemData != null)
        {
            _itemNameText.text = itemData.objectName;
        }
    }

    public void SetItemNameVisible(bool visible)
    {
        if (_itemNameText != null)
            _itemNameText.gameObject.SetActive(visible);
    }

    public void PickUp()
    {
        // WIP : envoyer vers la voiture
        Destroy(gameObject);
    }

    public void Rotate()
    {
        // WIP
    }
}
