using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Управляет плавным затемнением и осветлением экрана
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

            Color currentColor = fadeImage.color;
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
        }
    }

    private void Start()
    {
        FadeFromBlack();
    }

    private void OnLevelWasLoaded(int level)
    {
        Time.timeScale = 1;

        FadeFromBlack();
    }

    public void FadeToBlackSimple()
    {
        StartCoroutine(FadeRoutine(1f, null, afterFade: null));
    }

    public Coroutine FadeToBlack(string sceneName = null, LoadSceneMode mode = LoadSceneMode.Single, Action afterFade = null)
    {
        return StartCoroutine(FadeRoutine(1f, sceneName, mode, afterFade)); // 1f = полностью непрозрачный
    }

    public Coroutine FadeFromBlack(Action afterFade = null)
    {
        return StartCoroutine(FadeRoutine(0f, null, afterFade: afterFade)); // 0f = полностью прозрачный
    }

    private IEnumerator FadeRoutine(float targetAlpha, string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action afterFade = null)
    {
        yield return new WaitForSecondsRealtime(.5f);

        Color currentColor = fadeImage.color;
        float startAlpha = currentColor.a;
        float time = 0;

        // Включаем картинку перед началом анимации
        fadeImage.enabled = true;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        // Устанавливаем финальное значение, чтобы избежать погрешностей
        fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

        // Вызываем action после фейда, перед загрузкой
        afterFade?.Invoke();

        // Если экран стал полностью прозрачным, то выключаем картинку
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