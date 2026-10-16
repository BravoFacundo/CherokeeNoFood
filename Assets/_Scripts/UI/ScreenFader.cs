using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Full screen black image on top of the canvas. Starts black, fades in, and fades out before leaving.
[RequireComponent(typeof(Image))]
public class ScreenFader : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.6f;
    [Tooltip("Frames kept in black before fading in, so layouts settle")]
    [SerializeField] private int settleFrames = 2;

    private Image image;
    private Coroutine fadeRoutine;

    public bool IsFading => fadeRoutine != null;

    private void Awake()
    {
        image = GetComponent<Image>();
        SetAlpha(1f); // Always starts black
    }

    public void FadeIn() => Run(FadeInRoutine());
    public void FadeOut(Action onComplete) => Run(Fade(image.color.a, 1f, fadeOutDuration, onComplete));

    private void Run(IEnumerator routine)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(routine);
    }

    private IEnumerator FadeInRoutine()
    {
        for (int i = 0; i < settleFrames; i++) yield return null;
        yield return Fade(1f, 0f, fadeInDuration, null);
    }

    private IEnumerator Fade(float from, float to, float duration, Action onComplete)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsedTime / duration));
            yield return null;
        }
        SetAlpha(to);
        fadeRoutine = null;
        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
        image.raycastTarget = alpha > 0f; // Blocks clicks while visible
    }
}
