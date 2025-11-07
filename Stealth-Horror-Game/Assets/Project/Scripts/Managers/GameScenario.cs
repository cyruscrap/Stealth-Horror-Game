using System.Collections.Generic;
using UnityEngine;
using UnityTutorial.PlayerControl;
using Zenject;

/// <summary>
/// Управляет состоянием игры
/// </summary>
public class GameScenario : IInitializable, System.IDisposable
{
    // Зависимости
    private readonly List<DayData> _allDaysData;
    private readonly EventManager _eventManager;
    private readonly PlayerController _playerController;

    // Состояние
    private int _currentDayIndex = 0;
    private List<ItemData> _requiredItems;
    private int _itemsCollectedCount = 0;

    public static GameScenario Instance { get; private set; }

    public GameScenario(EventManager eventManager, PlayerController playerController)
    {
        _eventManager = eventManager;
        _playerController = playerController;
    }

    public void Initialize()
    {
        _eventManager.OnMiniGameCompleted += OnMiniGameCompleted;
    }

    public void Dispose()
    {
        _eventManager.OnMiniGameCompleted -= OnMiniGameCompleted;
    }

    private void StartDay(int dayIndex)
    {
        Debug.Log("Начинаем день " + dayIndex);

        _itemsCollectedCount = 0;
        //_itemSpawner.ClearAndHideAllItems();

        var currentDay = _allDaysData[dayIndex];
        _requiredItems = currentDay.GetRequiredItems();

        //_itemSpawner.SpawnItemsForDay(currentDay.itemsToSpawn, currentDay.enableOutline, currentDay.outlineColor, currentDay.outlineWidth, _mainGameRoot);

        // Перемещаем игрока
        _playerController.gameObject.SetActive(true);
        //_playerController.SetPosition(currentDay.playerStartPosition);
        //_playerController.SetRotation(currentDay.playerStartRotation);

        // Используем задержку без корутин в GameScenario
        _eventManager.ReportDayStart(currentDay);
        ScreenFaderTransition.Instance.FadeFromBlack();
    }

    public DayData GetCurrentDayData()
    {
        if (_currentDayIndex >= 0 && _currentDayIndex < _allDaysData.Count)
        {
            return _allDaysData[_currentDayIndex];
        }
        return null;
    }

    private void OnMiniGameCompleted()
    {
        ResumeMainGame();
    }

    public void PauseMainGame()
    {
        // Отключаем контроллер
        _playerController.gameObject.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeMainGame(bool fadeFromBlack = true, bool enablePlayerController = true)
    {
        if (enablePlayerController)
        {
            // Включаем контроллер
            _playerController.gameObject.SetActive(true);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (fadeFromBlack)
        {
            // Добавляем плавное появление основной сцены
            ScreenFaderTransition.Instance.FadeFromBlack();
        }
    }
}
