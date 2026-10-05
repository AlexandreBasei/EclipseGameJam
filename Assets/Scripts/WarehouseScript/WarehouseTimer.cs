using TMPro;
using UnityEngine;

public class WarehouseTimer : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI timerText;
    public float targetTime = 10.0f;

    private bool timerDone = false;

    private void Start()
    {
        timerText.text = "Time left: " + Mathf.Round(targetTime).ToString() + "s";
    }

    private void Update()
    {
        if (!timerDone)
        {
            targetTime -= Time.deltaTime;

            if (targetTime <= 0.0f)
            {
                timerEnded();
            }
            timerText.text = "Time left: " + Mathf.Max(0, Mathf.Round(targetTime)).ToString() + "s";
        }
    }

    void timerEnded()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.carsStartSound);
        timerDone = true;
        goToWorkShop();
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

    

    
}
