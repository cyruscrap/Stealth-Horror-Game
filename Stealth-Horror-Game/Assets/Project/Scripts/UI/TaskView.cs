using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TaskView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI taskText;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(string textKey)
    {
        string localizedText = I2.Loc.LocalizationManager.GetTranslation(textKey);
        taskText.text = string.IsNullOrEmpty(localizedText) ? textKey : localizedText;

        _canvasGroup.alpha = 0f;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0.5f)); // ‘ейд-ин за 0.5 сек
    }

    public IEnumerator FadeOutAndDestroy(float duration = 0.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        yield return FadeRoutine(0f, duration);
        Destroy(gameObject);
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = _canvasGroup.alpha;
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        _canvasGroup.alpha = targetAlpha;
        _fadeCoroutine = null;
    }
}