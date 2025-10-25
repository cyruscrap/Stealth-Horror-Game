using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }

    [SerializeField] private RectTransform hintTextTransform;
    [SerializeField] private TextMeshProUGUI commentText;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector3 offset = new Vector3(0, 40, 0); // Смещение подсказки относительно центра объекта
    [SerializeField] private float fadeTime = 1.0f;
    [SerializeField] private float commentTextTime = 5f;

    private Transform currentTarget;
    private Color commentTextColor;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Убедимся, что подсказка выключена
        if (hintTextTransform != null)
        {
            hintTextTransform.gameObject.SetActive(false);
        }

        if (commentText != null)
        {
            commentTextColor = commentText.color;
            commentText.color = new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0);
            commentText.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // В LateUpdate, чтобы позиция объекта уже была вычислена
        if (currentTarget != null && hintTextTransform.gameObject.activeSelf)
        {
            // Преобразуем мировую позицию цели в позицию на экране
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(currentTarget.position);
            hintTextTransform.position = screenPoint + offset;
        }
    }

    public void ShowHint(Transform target)
    {
        currentTarget = target;
        if (hintTextTransform != null)
            hintTextTransform.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        currentTarget = null;
        if (hintTextTransform != null)
            hintTextTransform.gameObject.SetActive(false);
    }

    public void ShowComment(string newCommentText)
    {
        StopAllCoroutines();
        if (commentText != null)
        {
            commentText.gameObject.SetActive(true);
            commentText.text = newCommentText;
            StartCoroutine(FadeComment(fadeTime, true));
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

    IEnumerator FadeComment(float fadeDuration, bool fadeOut)
    {
        Color startColor = fadeOut ? new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0) : commentTextColor;
        Color endColor = fadeOut ? commentTextColor : new Color(commentTextColor.r, commentTextColor.g, commentTextColor.b, 0);

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