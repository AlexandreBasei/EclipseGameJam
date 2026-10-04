using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private int actualMoney;
    [SerializeField] private float animationDuration = 0.5f;

    private int displayedMoney;
    private Coroutine moneyAnimation;

    public void UpdateMoney(int valueToAdd)
    {
        actualMoney += valueToAdd;
        NewMoney(actualMoney);
    }

    private void NewMoney(int newMoney)
    {
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

            yield return null;
        }

        displayedMoney = targetMoney;
        moneyText.text = displayedMoney + " $";

        moneyAnimation = null;
    }
}