using UnityEngine;
using TMPro;

public class CarDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;
    public TextMeshProUGUI ItemNameText => null;
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
            goToWareHouse();
        }
        else
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.carsStartSound);
            goToWorkShop();
        }

    }

    private void goToWareHouse()
    {
        FadeInOut.Instance.FadeIn();
        Invoke(nameof(goToWareHouseEnd), 1f);

    }

    private void goToWareHouseEnd()
    {
        DaysManager.Instance.loadWarehouseScene();
        FadeInOut.Instance.FadeOut();
    }

    
    private void goToWorkShop()
    {
        FadeInOut.Instance.FadeIn();
        Invoke(nameof(goToWorkShopEnd), 1f);
        
    }

    private void goToWorkShopEnd()
    {
        DaysManager.Instance.loadWorkShopScene();
        FadeInOut.Instance.FadeOut();
    }

    public void SetItemNameVisible(bool visible)
    {
        // Ne rien faire
    }
}
