using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }

    [SerializeField] private RectTransform hintTextTransform;
    [SerializeField] private TextMeshProUGUI commentText;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 40, 0);
    [SerializeField] private float fadeTime = 1.0f;
    [SerializeField] private float commentTextTime = 5f;

    [Header("Sequence")]
    [Tooltip("Пауза между гаснущей и появляющейся репликой")]
    [SerializeField] private float sequenceGap = 0.15f;
    [Tooltip("Клик во время реплики — мгновенно показать следующую")]
    [SerializeField] private bool allowSkipOnClick = true;

    private Vector3 currentTargetPos;
    private Color commentTextColor;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (hintTextTransform != null) hintTextTransform.gameObject.SetActive(false);
        if (commentText != null)
        {
            commentTextColor = commentText.color;
            commentText.color = new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0);
            commentText.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (currentTargetPos != Vector3.zero && hintTextTransform.gameObject.activeSelf)
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(currentTargetPos);
            hintTextTransform.position = screenPoint + offset;
        }
    }

    public void ShowHint(Vector3 targetPos)
    {
        currentTargetPos = targetPos;
        if (hintTextTransform != null) hintTextTransform.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        currentTargetPos = Vector3.zero;
        if (hintTextTransform != null) hintTextTransform.gameObject.SetActive(false);
    }

    // --- Одиночный комментарий (как было) ---

    public void ShowComment(string newCommentTextKey, float delay = 0)
    {
        StopAllCoroutines();
        if (commentText != null)
        {
            string text = LocalizationManager.GetTranslation(newCommentTextKey);
            if (string.IsNullOrEmpty(text)) text = newCommentTextKey;

            commentText.gameObject.SetActive(true);
            commentText.text = text;
            StartCoroutine(FadeComment(fadeTime, true, delay));
        }
    }

    public void HideComment()
    {
        if (commentText != null)
        {
            commentText.text = "";
            StartCoroutine(FadeComment(fadeTime, false));
        }
    }

    // --- Последовательность комментариев ---

    public void ShowCommentSequence(List<string> keys, float timePerLine = -1f)
    {
        if (keys == null || keys.Count == 0) return;
        StopAllCoroutines();
        StartCoroutine(CommentSequenceRoutine(keys, timePerLine < 0f ? commentTextTime : timePerLine));
    }

    private IEnumerator CommentSequenceRoutine(List<string> keys, float timePerLine)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (string.IsNullOrEmpty(key)) continue;

            string text = LocalizationManager.GetTranslation(key);
            if (string.IsNullOrEmpty(text)) text = key;

            // Подготовка: показать элемент, alpha = 0
            commentText.gameObject.SetActive(true);
            commentText.text = text;
            commentText.color = new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0);

            // Фейд-ин
            yield return FadeAlpha(0f, 1f, fadeTime);

            // Держим реплику, ожидая либо истечения времени, либо клика
            float elapsed = 0f;
            bool skipped = false;
            while (elapsed < timePerLine)
            {
                if (allowSkipOnClick && Input.GetMouseButtonDown(0))
                {
                    skipped = true;
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Фейд-аут
            yield return FadeAlpha(1f, 0f, fadeTime);

            // Небольшая пауза между репликами
            if (i < keys.Count - 1 && !skipped)
                yield return new WaitForSeconds(sequenceGap);
        }

        commentText.text = "";
        commentText.gameObject.SetActive(false);
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        Color c = commentTextColor;
        c.a = from;
        commentText.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            commentText.color = c;
            yield return null;
        }
        c.a = to;
        commentText.color = c;
    }

    // --- Старый FadeComment (для одиночных реплик) ---

    IEnumerator FadeComment(float fadeDuration, bool fadeOut, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        Color startColor = fadeOut
            ? new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0)
            : commentTextColor;
        Color endColor = fadeOut
            ? commentTextColor
            : new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0);

        float timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            commentText.color = Color.Lerp(startColor, endColor, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        commentText.color = endColor;

        if (fadeOut)
        {
            yield return new WaitForSeconds(fadeDuration);
            StartCoroutine(FadeComment(commentTextTime, false));
        }
        else
        {
            commentText.gameObject.SetActive(false);
        }
    }
}