using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// ��������� ������� ����������� � ����������� ������
/// </summary>
public class ScreenFaderTransition : MonoBehaviour
{
    public static ScreenFaderTransition Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var currentColor = fadeImage.color;
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
        }
    }

    private void Start()
    {
        FadeFromBlack();
    }

    public Coroutine FadeToBlack(string sceneName = null, LoadSceneMode mode = LoadSceneMode.Single, Action afterFade = null)
    {
        return StartCoroutine(FadeRoutine(1f, sceneName, mode, afterFade)); // 1f = ��������� ������������
    }

    public Coroutine FadeFromBlack(Action afterFade = null)
    {
        return StartCoroutine(FadeRoutine(0f, null, afterFade: afterFade)); // 0f = ��������� ����������
    }

    private IEnumerator FadeRoutine(float targetAlpha, string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action afterFade = null)
    {
        yield return new WaitForSeconds(.5f);

        var currentColor = fadeImage.color;
        var startAlpha = currentColor.a;
        float time = 0;

        // �������� �������� ����� ������� ��������
        fadeImage.enabled = true;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            var newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        // ������������� ��������� ��������, ����� �������� ������������
        fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

        // �������� action ����� �����, ����� ���������
        afterFade?.Invoke();

        // ���� ����� ���� ��������� ����������, �� ��������� ��������
        if (targetAlpha == 0)
        {
            fadeImage.enabled = false;
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
            yield return asyncLoad;
        }
    }
}