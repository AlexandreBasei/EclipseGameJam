using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private RectTransform mainMenuRect;
    [SerializeField] private RectTransform creditRect;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float offscreenX = 1920f;

    private Vector3 _mainMenuStartPos;
    private Vector3 _creditStartPos;

    public void PlayGameOnClick()
    {
        SceneManager.LoadScene("MainScene");
        // AudioManager.Instance.PlayGameMusic();
    }

    public void OpenCreditOnClick()
    {
        LeanTween.move(mainMenuRect.gameObject, new Vector3(-offscreenX, 0f, 0f), moveDuration).setEaseOutCubic();
        LeanTween.move(creditRect.gameObject, new Vector3(0, 0f, 0f), moveDuration).setEaseOutCubic();
    }

    public void CloseCreditsOnClick()
    {
        LeanTween.move(creditRect.gameObject, new Vector3(offscreenX, 0f, 0f), moveDuration).setEaseOutCubic();
        LeanTween.move(mainMenuRect.gameObject, new Vector3(0f, 0f, 0f), moveDuration).setEaseOutCubic();
    }

    public void QuitGameOnClick()
    {
        Application.Quit();
    }
}