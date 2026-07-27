using UnityEngine;
using System.Collections;

/// <summary>
/// District banner: fades in 0.3s, holds 1.8s, fades out 0.6s.
/// Drives CanvasGroup.alpha via coroutine.
/// </summary>
public class DistrictBannerController : MonoBehaviour
{
    [Header("Timing (seconds)")]
    public float fadeInDuration = 0.3f;
    public float holdDuration = 1.8f;
    public float fadeOutDuration = 0.6f;

    private CanvasGroup canvasGroup;
    private Coroutine currentRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        Show();
    }

    public void Show()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(BannerRoutine());
    }

    /// <summary>
    /// Returns the alpha timeline for testing (no coroutine needed).
    /// Given elapsed time, returns expected alpha.
    /// </summary>
    public float GetExpectedAlpha(float elapsed)
    {
        if (elapsed < 0) return 0f;

        if (elapsed < fadeInDuration)
        {
            // 0 -> 1 linear
            return Mathf.Clamp01(elapsed / fadeInDuration);
        }
        else if (elapsed < fadeInDuration + holdDuration)
        {
            // Hold at 1
            return 1f;
        }
        else if (elapsed < fadeInDuration + holdDuration + fadeOutDuration)
        {
            // 1 -> 0 linear
            float t = (elapsed - fadeInDuration - holdDuration) / fadeOutDuration;
            return Mathf.Clamp01(1f - t);
        }
        else
        {
            return 0f;
        }
    }

    IEnumerator BannerRoutine()
    {
        // Fade in
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1f - (t / fadeOutDuration));
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
