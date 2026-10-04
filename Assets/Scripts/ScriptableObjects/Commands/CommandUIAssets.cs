using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "CommandUIAssets", menuName = "UI/Command UI Assets")]
public class CommandUIAssets : ScriptableObject
{
    public VisualTreeAsset commandComponent;
    public VisualTreeAsset objectComponent;
    public VisualTreeAsset tagComponent;
}