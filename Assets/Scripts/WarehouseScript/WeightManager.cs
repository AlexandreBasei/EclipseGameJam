using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeightManager : Singleton<WeightManager>
{
    [SerializeField] private Image ImageBarFill;
    public Slider SliderBar;
    [SerializeField] private TextMeshProUGUI FullText;

    private void Start()
    {
        OnSliderChanged(SliderBar.value);
    }

    public void AddSliderValue(float addValue)
    {
        SliderBar.value += addValue;
    }

    public void OnSliderChanged(float value)
    {
        float maxValue = SliderBar.maxValue;
        float pourcentage = (value / maxValue) * 100f;

        if (value >= SliderBar.maxValue)
        {
            FullText.text = "FULL";
            FullText.color = Color.red;
            print("Full");
        }
        else
        {
            FullText.text = " ";
             print("Vide");
        }

        ImageBarFill.enabled = value > 0;

        if (value >= maxValue)
        {
            ImageBarFill.color = Color.red;
        }
        else if (pourcentage >= 50f)
        {
            ImageBarFill.color = new Color(1f, 0.5f, 0f);
        }
        else
        {
            ImageBarFill.color = Color.green;
        }

        


    }
}