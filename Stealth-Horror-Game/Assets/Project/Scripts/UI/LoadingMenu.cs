using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingMenu : MonoBehaviour
{
    [Header("RectTransform (заменитель курсора)")]
    [SerializeField] private RectTransform cursorTransform;

    [Header("Отступы от левого и нижнего края (в пикселях)")]
    [SerializeField] private Vector2 padding = new Vector2(100f, 100f);

    [Header("Размер курсора (ширина, высота)")]
    [SerializeField] private Vector2 cursorSize = new Vector2(72f, 72f);

    [Header("Параметры анимации курсора")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Параметры анимации текста")]
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] string fullText = "LOADING";
    [SerializeField] float typeDelay = 0.2f;

    [Header("Следующая сцена")]
    [SerializeField] string mainSceneName = "MainScene";

    private MainMenu mainMenu;
    private RectTransform imageRect;
    private bool isAnimating = false;

    private void Awake()
    {
        mainMenu = GetComponent<MainMenu>();
    }

    private void Start()
    {
        InitializeCanvasAndImage();
        cursorTransform.gameObject.SetActive(false);
    }

    /// <summary>
    /// Буквы поочердно сменяют друг друга внутри курсора перед загрузкой новой сцены
    /// </summary>
    public void StartTextAnimation()
    {
        StartCoroutine(AnimateTextTyping());
    }

    private IEnumerator AnimateTextTyping()
    {
        // 1. Запускаем асинхронную загрузку сцены в фоне
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainSceneName);

        // Запрещаем игре переключать сцену сразу, как только она загрузится
        asyncLoad.allowSceneActivation = false;

        // 2. Проигрываем твою анимацию печати, пока сцена грузится
        textMeshPro.text = "";
        foreach (char letter in fullText)
        {
            textMeshPro.text = letter.ToString();
            yield return new WaitForSecondsRealtime(typeDelay);
        }

        // 3. Ждем, пока сцена загрузится хотя бы на 90% (при allowSceneActivation = false прогресс замирает на 0.9)
        // Если сцена загрузилась быстрее анимации — этот цикл пропустится моментально.
        // Если сцена грузится дольше — игра подождет.
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 4. Когда и анимация, и сцена готовы, делаем затемнение.
        // Передаем null вместо имени сцены, так как мы уже загрузили её вручную.
        // В коллбэке afterFade разрешаем активацию сцены, чтобы произошло переключение.
        ScreenFaderTransition.Instance.FadeToBlack(null, afterFade: () =>
        {
            asyncLoad.allowSceneActivation = true;
        });
    }

    /// <summary>
    /// Запускает анимацию: UI-курсор перемещается из текущей позиции мыши в левый нижний угол,
    /// после чего системный курсор скрывается и блокируется.
    /// </summary>
    public void StartCursorAnimation()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateCursorReplacement());
    }

    private IEnumerator AnimateCursorReplacement()
    {
        isAnimating = true;

        // Показываем UI Image и размещаем его в текущей позиции мыши
        cursorTransform.gameObject.SetActive(true);
        Vector2 startScreenPos = Input.mousePosition;
        SetUIPosition(startScreenPos);

        // Целевая позиция в левом нижнем углу с отступами
        Vector2 targetScreenPos = new Vector2(padding.x, padding.y);

        // Скрываем системный курсор, но пока не блокируем (чтобы можно было считывать движение)
        Cursor.visible = false;
        // Не блокируем, чтобы позиция мыши не сбрасывалась в центр
        Cursor.lockState = CursorLockMode.None;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            float curve = movementCurve.Evaluate(t);
            Vector2 currentPos = Vector2.Lerp(startScreenPos, targetScreenPos, curve);
            SetUIPosition(currentPos);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Фиксируем UI в конечной позиции
        SetUIPosition(targetScreenPos);

        // Теперь полностью блокируем системный курсор (игрок не сможет его двигать)
        Cursor.lockState = CursorLockMode.Locked;
        // Он уже скрыт, управление отключено

        isAnimating = false;

        StartTextAnimation();
    }

    // Устанавливает позицию UI Image по экранным координатам
    private void SetUIPosition(Vector2 screenPos)
    {
        if (imageRect == null) return;
        // Переводим экранные координаты в локальные координаты Canvas (для ScreenSpaceOverlay)
        RectTransform canvasRect = transform as RectTransform;
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out localPos);
        imageRect.anchoredPosition = localPos;
    }

    // Настройка Canvas и Image (вызывается один раз)
    private void InitializeCanvasAndImage()
    {
        /*
        // Убеждаемся, что скрипт висит на Canvas (или создаём его)
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("CustomCursorCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
                // Переносим этот скрипт на созданный Canvas
                transform.SetParent(canvasGO.transform);
            }
        }

        // Если Image не назначен – создаём
        if (cursorImage == null)
        {
            GameObject imageGO = new GameObject("CustomCursorImage");
            imageGO.transform.SetParent(transform, false);
            cursorImage = imageGO.AddComponent<Image>();
            cursorImage.color = Color.white; // задайте свой спрайт в инспекторе
        }
        */

        // Настраиваем RectTransform: якоря по центру, чтобы позиция задавалась через anchoredPosition
        imageRect = cursorTransform;
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        if (cursorSize != Vector2.zero)
            imageRect.sizeDelta = cursorSize;

        cursorTransform.gameObject.SetActive(false);
    }
}