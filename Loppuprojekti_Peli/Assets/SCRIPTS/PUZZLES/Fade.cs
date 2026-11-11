using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Image))]
public class UIImageFader : MonoBehaviour
{
    [Header("Asetukset")]
    public float fadeDuration = 1.0f;
    public float startDelay = 0.0f;

    [Header("Scenen vaihto")]
    public string levelName = "Level2";
    public float delayAfterFade = 1.0f;

    [Header("Automaattinen käynnistys")]
    public bool fadeInOnStart = false;
    public bool fadeOutOnStart = false;

    private Image targetImage;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        targetImage = GetComponent<Image>();

        // Piilotetaan Image-komponentti heti alussa
        targetImage.enabled = false;
    }

    private void Start()
    {
        if (fadeInOnStart) FadeIn();
        else if (fadeOutOnStart) FadeOut();
    }

    public void FadeIn()
    {
        targetImage.enabled = true;
        StartFade(0f, 1f, fadeDuration, startDelay);
    }

    public void FadeInAndLoadScene()
    {
        StartCoroutine(FadeInAndLoadRoutine());
    }

    private IEnumerator FadeInAndLoadRoutine()
    {
        Debug.Log("1. Aloitetaan häivytys kohti sceneä: " + levelName);
        targetImage.enabled = true;
        StartFade(0f, 1f, fadeDuration, startDelay);

        if (currentFadeCoroutine != null)
        {
            yield return currentFadeCoroutine;
        }

        Debug.Log("2. Häivytys valmis, odotetaan " + delayAfterFade + " sekuntia.");
        yield return new WaitForSecondsRealtime(delayAfterFade);

        Debug.Log("3. Ladataan scene: " + levelName);
        SceneManager.LoadScene(levelName);
    }


    public void FadeOut()
    {
        targetImage.enabled = true;
        StartFade(1f, 0f, fadeDuration, startDelay);
    }

    public void StartFade(float startAlpha, float endAlpha, float duration, float delay)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeRoutine(startAlpha, endAlpha, duration, delay));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, float delay)
    {
        SetAlpha(startAlpha);

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            SetAlpha(newAlpha);
            yield return null;
        }

        SetAlpha(endAlpha);
        currentFadeCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = targetImage.color;
        color.a = Mathf.Clamp01(alpha);
        targetImage.color = color;
    }
}
