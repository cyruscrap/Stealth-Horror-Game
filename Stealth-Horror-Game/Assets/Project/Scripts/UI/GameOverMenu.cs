using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameOverMenu : MonoBehaviour
{
    [Inject] private WorldSaveGameManager _worldSaveGameManager;
    [SerializeField] private LoadingMenu loadingMenu; // ссылка на компонент LoadingMenu (если он на сцене)

    private void Start()
    {
        gameObject.SetActive(false);
    }

    // Вызывается кнопкой "Продолжить с последнего сохранения"
    public void ContinueFromLastSave()
    {
        // 1. Загружаем данные активного слота
        _worldSaveGameManager.SetCurrentSlot(_worldSaveGameManager.CurrentSlotIndex); // можно не вызывать, слот уже активен
        _worldSaveGameManager.LoadGame();

        // 2. Возвращаем нормальное время и запускаем загрузку сцены
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 3. Используем LoadingMenu для перехода на текущую сцену
        if (loadingMenu != null)
        {
            loadingMenu.StartCursorAnimation(); // внутри он вызывает FadeToBlack с именем сцены
        }
        else
        {
            // Запасной вариант — просто перезагрузить активную сцену
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}