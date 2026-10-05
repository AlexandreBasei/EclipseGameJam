using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyUI : Singleton<MoneyUI>
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private float animationDuration = 0.5f;

    private int displayedMoney;
    private Coroutine moneyAnimation;

    private void Start()
    {
        NewMoney(DaysManager.Instance.MoneyValue);
    }

    public void UpdateMoney(int valueToAdd)
    {
        NewMoney(DaysManager.Instance.MoneyValue + valueToAdd);
    }

    public void NewMoney(int newMoney)
    {
        DaysManager.Instance.SetMoneyValue(newMoney);
        PlayerHUD.Instance.ChangedMoneyValue(newMoney);

        if (moneyAnimation != null)
            StopCoroutine(moneyAnimation);

        moneyAnimation = StartCoroutine(AnimateMoney(newMoney));
    }

    private IEnumerator AnimateMoney(int targetMoney)
    {
        int startMoney = displayedMoney;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            float percentage = elapsedTime / animationDuration;

            displayedMoney = Mathf.RoundToInt(Mathf.Lerp(startMoney, targetMoney, percentage));

            moneyText.text = displayedMoney + " $";

            UpdateMoneyColor();

            yield return null;
        }

        displayedMoney = targetMoney;
        moneyText.text = displayedMoney + " $";

        UpdateMoneyColor();

        moneyAnimation = null;
    }

    private void UpdateMoneyColor()
    {
        if (displayedMoney > 0)
            moneyText.color = new Color(47f / 255f, 183f / 255f, 79f / 255f);
        else if (displayedMoney < 0)
            moneyText.color = Color.red;

    }
}
