using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SecretTeamPopup : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // В начале игры скрываем панель полностью
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        // Проверяем комбинацию: удерживаются Ctrl + Shift И нажата клавиша T
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
            (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
            Input.GetKeyDown(KeyCode.T))
        {
            if (!isShowing)
            {
                StartCoroutine(ShowAndFadeRoutine());
            }
        }
    }

    IEnumerator ShowAndFadeRoutine()
    {
        isShowing = true;

        // 1. Мгновенно показываем имена
        canvasGroup.alpha = 1f;

        // 2. Ждем ровно 3 секунды (можете изменить это число)
        yield return new WaitForSeconds(3f);

        // 3. Плавно угасаем за 1.5 секунды
        float fadeDuration = 1.5f;
        float currentTime = 0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            yield return null;
        }

        // Убеждаемся, что альфа точно на нуле
        canvasGroup.alpha = 0f;
        isShowing = false;
    }
}
