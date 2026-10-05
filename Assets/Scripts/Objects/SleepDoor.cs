using UnityEngine;

public class SleepDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private Color _highlightedOutlineColor = Color.blue;
    public Color OutlineColor => _outlineColor;
    public Color HighlightedOutlineColor => _highlightedOutlineColor;

    public void PickUp(Camera playerCamera = null)
    {
        if(DaysManager.Instance.currentDay == 5)
        {
            EndingScript.Instance.EndGame();
            return;
        }
        if(DaysManager.Instance.tutoFinished == false)
        {
            return;
        }
        FadeInOut.Instance.FadeIn();
        AudioManager.Instance.PlaySFX(AudioManager.Instance.coqSound);
        DaysManager.Instance.NextDay();
        FadeInOut.Instance.FadeOut();
    }

    public void SetItemNameVisible(bool visible)
    {
        return;
    }
}
