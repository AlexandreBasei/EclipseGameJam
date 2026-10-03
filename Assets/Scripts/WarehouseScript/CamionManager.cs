using UnityEngine;
using UnityEngine.UI;

public class CamionManager : MonoBehaviour
{

    [SerializeField] private Slider weightSlider;

    [SerializeField] private Color lowWeightColor = Color.green;
    [SerializeField] private Color mediumWeightColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color maxWeightColor = Color.red;

    [Range(0f, 1f)]
    [SerializeField] private float mediumThreshold = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float highThreshold = 0.8f;

    public int weightMax = 5;
    public int weightActual = 0;

    private Image fillImage;

    private void Awake()
    {
        if (weightSlider != null)
            fillImage = weightSlider.fillRect.GetComponent<Image>();
    
    }

    public void UpdateWeight(int weightToAdd)
    {
        weightActual += weightToAdd;
        UpdateWeightUI(weightActual, weightMax);   
    }

    public void UpdateWeightUI(int currentWeight, int maxWeight)
    {
        if (weightSlider == null || maxWeight <= 0)
            return;


        float percentage = Mathf.Clamp01((float)currentWeight / maxWeight);

        weightSlider.minValue = 0f;
        weightSlider.maxValue = 1f;
        weightSlider.value = percentage;

        UpdateColorUI(percentage);
    }

    private void UpdateColorUI(float percentage)
    {
        if (fillImage == null)
            return;

        if (percentage < mediumThreshold)
        {
            fillImage.color = lowWeightColor;
        }
        else if (percentage < highThreshold)
        {
            fillImage.color = mediumWeightColor;
        }
        else
        {
            fillImage.color = maxWeightColor;
        }
    }
}
