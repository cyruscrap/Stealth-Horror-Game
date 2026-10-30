using System.Collections;
using UnityEngine;
using Zenject;

public enum SlotMode
{
    NewGame,
    LoadGame
}

public class MainMenu : MonoBehaviour
{
    LoadingMenu loadingMenu;

    [Inject] WorldSaveGameManager _worldSaveGameManager;

    [Header("Menus")]
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup initMenu;
    [SerializeField] private GameObject slotsPanel;
    [SerializeField] private GameObject confirmOverwritePanel;
    [SerializeField] private CanvasGroup introduction;
    //[SerializeField] private TextMeshProUGUI introductionText;

    [SerializeField] float menuDelay = 2;
    [SerializeField] float menuFadeDuration = 1;
    [SerializeField] bool showStartMenu = true;

    [Header("Save Slots")]
    [SerializeField] private SaveSlotButton[] saveSlotButtons;

    private SlotMode _currentSlotMode;

    private void Awake()
    {
        loadingMenu = GetComponent<LoadingMenu>();
    }

    private void Start()
    {
        if (showStartMenu)
            initMenu?.gameObject.SetActive(true);

        // Скрываем панель слотов до момента выбора действия
        if (slotsPanel != null)
            slotsPanel.SetActive(false);
    }

    // Вызывается кнопкой "Новая игра"
    public void NewGame()
    {
        _currentSlotMode = SlotMode.NewGame;
        ShowSlotsPanel();
    }

    // Вызывается кнопкой "Загрузить игру"
    public void LoadGame()
    {
        _currentSlotMode = SlotMode.LoadGame;
        ShowSlotsPanel();
    }

    private void ShowSlotsPanel()
    {
        if (slotsPanel != null)
            slotsPanel.SetActive(true);

        var slotsInfo = _worldSaveGameManager.GetSaveSlotsInfo();
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            saveSlotButtons[i].Initialize(slotsInfo[i].slotIndex, slotsInfo[i], _worldSaveGameManager, OnSlotClicked);
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (_currentSlotMode == SlotMode.NewGame)
            StartNewGameInSlot(slotIndex);
        else if (_currentSlotMode == SlotMode.LoadGame)
            LoadGameFromSlot(slotIndex);
    }

    private void StartNewGameInSlot(int slotIndex)
    {
        // Устанавливаем активный слот
        _worldSaveGameManager.SetCurrentSlot(slotIndex);

        SaveSlotInfo info = _worldSaveGameManager.GetSaveSlotsInfo()[slotIndex];
        if (!info.isEmpty)
        {
            // Например, вызвать метод ShowConfirmation(slotIndex)
            confirmOverwritePanel.gameObject.SetActive(true);
            return;
        }
        else
        {
            ProceedWithNewGame();
        }
    }

    public void ProceedWithNewGame()
    {
        // Удаляем сейв файл слота
        _worldSaveGameManager.DeleteSaveFile();

        // Создаём новые данные (сброс)
        CharacterSaveData newData = new CharacterSaveData();
        newData.SetDefaultValues();
        _worldSaveGameManager.currentCharacterSaveData = newData;

        _worldSaveGameManager.SaveGame(1, PhaseType.Day);

        PlayerPrefsExtentions.SetBool("isNewGame", true);

        // Запускаем анимацию загрузки и переход на игровую сцену
        StartCoroutine(MenuFade(mainMenu, 1, 0, menuFadeDuration));
        loadingMenu.StartCursorAnimation();
    }

    private void LoadGameFromSlot(int slotIndex)
    {
        // Устанавливаем активный слот и загружаем данные
        _worldSaveGameManager.SetCurrentSlot(slotIndex);

        PlayerPrefsExtentions.SetBool("isNewGame", false);

        // Запускаем анимацию загрузки и переход на игровую сцену
        StartCoroutine(MenuFade(mainMenu, 1, 0, menuFadeDuration));
        loadingMenu.StartCursorAnimation();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowOrHideIntroduction(bool show)
    {
        if (show == true)
        {
            introduction.gameObject.SetActive(true);
            StartCoroutine(MenuFade(introduction, 0, 1, menuFadeDuration));
        }
        else
        {
            introduction.gameObject.SetActive(false);
        }
    }

    public void ShowOrHideInitMenu(bool show)
    {
        if (show == true)
        {
            initMenu.gameObject.SetActive(true);
            StartCoroutine(MenuFade(initMenu, 0, 1, menuFadeDuration));
        }
        else
        {
            StartCoroutine(MenuFade(mainMenu, 0, 1, menuFadeDuration));
        }
    }

    private IEnumerator MenuFade(CanvasGroup menu1, float startValue, float endValue, float duration, float delay = 0, CanvasGroup menu2 = null)
    {
        yield return new WaitForSecondsRealtime(delay);

        float startAlpha = startValue;
        float endAlpha = endValue;
        float time = 0;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            menu1.alpha = progress;
            yield return null;
        }

        if (menu2 == null) yield return null;
        else
        {
            startAlpha = endValue;
            endAlpha = startValue;
            time = 0;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float progress = Mathf.Lerp(startAlpha, endAlpha, time / duration);
                menu2.alpha = progress;
                yield return null;
            }
        }
    }
}