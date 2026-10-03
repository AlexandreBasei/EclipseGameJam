using UnityEngine;

public class CarDoor : MonoBehaviour, IInteractable
{
    public Color OutlineColor { get; } = Color.white;
    public Color HighlightedOutlineColor { get; } = Color.blue;
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

    public void Rotate()
    {
        // Ne rien faire
    }

    public void Snap()
    {
        // Ne rien faire
    }

    public void SetItemNameVisible(bool visible)
    {
        // Ne rien faire
    }
}
