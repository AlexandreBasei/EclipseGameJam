using UnityEngine;

public class CarDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    public bool isDoorOpen = false;

    public void PickUp(Camera playerCamera = null)
    {
        isDoorOpen = true;
        GetComponent<Outline>().enabled = false;
        PlayerHUD.Instance.HideAllInputTips();

        // Son démarrage de la voiture

        Invoke(nameof(goToWorkShop), 1.5f);
        
    }

    private void goToWorkShop()
    {
        DaysManager.Instance.loadWorkShopScene();
    }

    public void SetItemNameVisible(bool visible)
    {
        // Ne rien faire
    }
}
