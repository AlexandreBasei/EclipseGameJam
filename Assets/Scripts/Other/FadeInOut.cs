using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : Singleton<FadeInOut>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CurrentCommandTab currentCommandTab;

    public void FadeIn()
    {
        currentCommandTab.HideTab();
        StopAllCoroutines();
        StartCoroutine(Fade(0.6f, 1f));
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }

    public void AllBlack()
    {
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }
}