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
        if (DaysManager.Instance.isInWorkshop && DaysManager.Instance.hasVisitedWareHouse)
        {
            return;
        }
        if (DaysManager.Instance.tutoStarted && DaysManager.Instance.tutoProgress < 3)
        {
            return;
        }
        isDoorOpen = true;
        GetComponent<Outline>().enabled = false;
        PlayerHUD.Instance.HideAllInputTips();

        // Son démarrage de la voiture

        if (DaysManager.Instance.isInWorkshop)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.carsStartSound);
            Invoke(nameof(goToWareHouse), 1.5f);
        }
        else
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.carsStartSound);
            Invoke(nameof(goToWorkShop), 1.5f);
        }

    }

    private void goToWareHouse()
    {
        DaysManager.Instance.loadWarehouseScene();
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
