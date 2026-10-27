using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AnimatedPage : MonoBehaviour
{
    [Header("Идентификация")]
    [SerializeField] private int pageNumber;
    [SerializeField] private bool isAutoPrinted = true;

    [Header("Текст")]
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private string textKey;
    [SerializeField] private float charDelay = 0.03f;

    [Header("Фото")]
    [SerializeField] private RectTransform photoRoot;
    [SerializeField] private CanvasGroup photoCanvasGroup;
    [SerializeField] private Image photoImage;                   // если нужно менять спрайт
    [Tooltip("Стартовый множитель к целевому масштабу. 1.3 = фото влетает чуть больше и сжимается.")]
    [SerializeField] private float photoStartScaleMultiplier = 1.3f;
    [SerializeField] private float photoGlueDuration = 0.5f;
    [SerializeField] private AnimationCurve photoScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve photoAlphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Inject] private WorldSaveGameManager _saveManager;

    private Coroutine _routine;
    private Vector3 _photoTargetScale = Vector3.one;

    private bool IsPageSeen()
    {
        return _saveManager.currentCharacterSaveData.diaryPrintedSpreads.Contains(pageNumber);
    }

    private void MarkPageSeen()
    {
        var data = _saveManager.currentCharacterSaveData;
        if (!data.diaryPrintedSpreads.Contains(pageNumber))
        {
            data.diaryPrintedSpreads.Add(pageNumber);
            _saveManager.NotifyStateChanged();
        }
    }

    private void Awake()
    {
        // запоминаем «целевой» масштаб из инспектора — к нему будем анимировать
        if (photoRoot != null)
            _photoTargetScale = photoRoot.localScale;
    }

    private void OnEnable()
    {
        if (!isAutoPrinted) { ShowFinalStateImmediate(); return; }
        if (IsPageSeen()) { ShowFinalStateImmediate(); return; }

        _routine = StartCoroutine(PlayIntroSequence());
    }

    private void OnDisable()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        yield return null;   // пропускаем кадр — защита от прогрева/мигания

        MarkPageSeen();

        ResetVisuals();
        yield return TypewriterRoutine();
        yield return GluePhotoRoutine();

        OnPrintCompleted?.Invoke(pageNumber);
        _routine = null;
    }

    public event System.Action<int> OnPrintCompleted;

    private void ResetVisuals()
    {
        if (bodyText != null)
        {
            bodyText.text = LocalizationManager.GetTranslation(textKey);
            bodyText.maxVisibleCharacters = 0;
        }
        if (photoRoot != null)
        {
            photoRoot.localScale = _photoTargetScale * photoStartScaleMultiplier;
        }
        if (photoCanvasGroup != null)
        {
            photoCanvasGroup.alpha = 0f;
        }
    }

    private IEnumerator TypewriterRoutine()
    {
        string text = LocalizationManager.GetTranslation(textKey);
        if (bodyText == null || string.IsNullOrEmpty(text)) yield break;

        for (int i = 1; i <= text.Length; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(charDelay);
        }
    }

    private IEnumerator GluePhotoRoutine()
    {
        if (photoRoot == null) yield break;

        float t = 0f;
        while (t < photoGlueDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / photoGlueDuration);

            float scaleK = photoScaleCurve.Evaluate(k);
            float alphaK = photoAlphaCurve.Evaluate(k);

            // от стартового (большего) масштаба → к целевому
            float multiplier = Mathf.Lerp(photoStartScaleMultiplier, 1f, scaleK);
            photoRoot.localScale = _photoTargetScale * multiplier;

            if (photoCanvasGroup != null)
                photoCanvasGroup.alpha = alphaK;

            yield return null;
        }

        photoRoot.localScale = _photoTargetScale;
        if (photoCanvasGroup != null)
            photoCanvasGroup.alpha = 1f;
    }

    private void ShowFinalStateImmediate()
    {
        if (bodyText != null)
        {
            string text = LocalizationManager.GetTranslation(textKey);
            bodyText.text = text;
            bodyText.maxVisibleCharacters = text.Length;
        }
        if (photoRoot != null)
            photoRoot.localScale = _photoTargetScale;
        if (photoCanvasGroup != null)
            photoCanvasGroup.alpha = 1f;
    }

    public void SetPhotoSprite(Sprite sprite)
    {
        if (photoImage != null) photoImage.sprite = sprite;
    }
}