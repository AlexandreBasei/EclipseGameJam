using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingScript : Singleton<EndingScript>
{
    [SerializeField] private Image BadEnding;
    [SerializeField] private Image GoodEnding;
    [SerializeField] private Image MoneyEnding;
    [SerializeField] private Button buttonMenu;

    [SerializeField] private int goalToMoneyEnding = 2000;


    public void EndGame()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        FadeInOut.Instance.AllBlack();

        if(PlayerHUD.Instance.moneyValue < 0)
        {
            BadEnding.enabled = true;
        }
        else if(PlayerHUD.Instance.moneyValue > goalToMoneyEnding)
        {
            MoneyEnding.enabled = true;
        }
        else
        {
            GoodEnding.enabled = true;
        }
        buttonMenu.gameObject.SetActive(true);
    }

    public void Reset()
    {
        BadEnding.enabled = false;
       
        MoneyEnding.enabled = false;
       
        GoodEnding.enabled = false;
        
        buttonMenu.gameObject.SetActive(false);
        
    }
}
