using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTutorial.PlayerControl;
using Zenject;

/// <summary>
/// Управляет состоянием игры
/// </summary>
public class GameScenario : IInitializable, IDisposable
{
    // Зависимости
    private readonly List<DayData> _allPhases;
    private readonly EventManager _eventManager;
    private readonly PlayerController _playerController;
    private readonly PlayerUIManager _playerUIManager;
    private readonly WorldSaveGameManager _worldSaveGameManager;
    private readonly PlayerDataSaver _playerDataSaver;

    private CharacterSaveData _saveData;
    private DayData _currentPhase;

    // Состояние
    private bool _isTransitioning;
    private int _currentPhaseIndex = 0;
    private List<ItemData> _requiredItems;
    private int _itemsCollectedCount = 0;

    public static GameScenario Instance { get; private set; }

    public GameScenario(EventManager eventManager, PlayerController playerController, PlayerUIManager playerUIManager,
        List<DayData> allDaysData, WorldSaveGameManager worldSaveGameManager, PlayerDataSaver playerDataSaver)
    {
        _eventManager = eventManager;
        _playerController = playerController;
        _playerUIManager = playerUIManager;
        _allPhases = allDaysData;
        _worldSaveGameManager = worldSaveGameManager;
        _playerDataSaver = playerDataSaver;
    }

    public void Initialize()
    {
        _isTransitioning = false;
        _eventManager.OnMiniGameCompleted += OnMiniGameCompleted;
        _eventManager.OnPhaseEnd += OnPhaseEnd;

        _playerDataSaver.LoadCharacterData();
        _saveData = _worldSaveGameManager.currentCharacterSaveData;

        _currentPhaseIndex = Mathf.Clamp(_saveData.phaseIndex, 0, Mathf.Max(0, _allPhases.Count - 1));
        StartPhase(_currentPhaseIndex);
    }

    public void Dispose()
    {
        _eventManager.OnMiniGameCompleted -= OnMiniGameCompleted;
        _eventManager.OnPhaseEnd -= OnPhaseEnd;
    }

    private void StartPhase(int index)
    {
        //_itemsCollectedCount = 0;
        //_itemSpawner.ClearAndHideAllItems();

        Debug.Log($"[GS] StartPhase: index={index}, totalPhases={_allPhases.Count}");
        for (int i = 0; i < _allPhases.Count; i++)
            Debug.Log($"[GS]   phase[{i}] = {_allPhases[i].phaseID} ({_allPhases[i].phaseType}, ch={_allPhases[i].chapterNumber})");

        _currentPhaseIndex = index;
        _currentPhase = _allPhases[index];
        _saveData.phaseIndex = index;

        Debug.Log($"Начинаем фазу {_currentPhase.phaseID} ({_currentPhase.phaseType}), глава {_currentPhase.chapterNumber}");

        _requiredItems = _currentPhase.GetRequiredItems();

        if (PlayerPrefsExtentions.GetBool("isNewGame"))
        {
            _playerController.transform.position = _currentPhase.playerStartPosition;
            _playerController.transform.rotation = Quaternion.Euler(_currentPhase.playerStartRotation);
        }

        //_itemSpawner.SpawnItemsForDay(currentDay.itemsToSpawn, currentDay.enableOutline, currentDay.outlineColor, currentDay.outlineWidth, _mainGameRoot);

        _eventManager.ReportPhaseStart(_currentPhase);
        _worldSaveGameManager.NotifyStateChanged();
        ScreenFaderTransition.Instance.FadeFromBlack();
    }

    public void AdvanceToNextPhase()
    {
        if (_isTransitioning)
        {
            Debug.LogWarning("[GameScenario] AdvanceToNextPhase already in progress, ignored");
            return;
        }

        int next = _currentPhaseIndex + 1;
        if (next >= _allPhases.Count)
        {
            Debug.Log("Фазы закончились — конец игры");
            return;
        }

        _isTransitioning = true;

        _saveData.phaseIndex = next;
        _currentPhaseIndex = next;
        _currentPhase = _allPhases[next];

        SaveGame();

        ScreenFaderTransition.Instance.FadeToBlack(SceneManager.GetActiveScene().name);
    }

    private void OnPhaseEnd(DayData data) => AdvanceToNextPhase();
    private void OnMiniGameCompleted() => ResumeMainGame();

    public DayData GetCurrentPhase() => _currentPhase;

    public int GetCurrentChapterNumber()
        => _currentPhase != null ? _currentPhase.chapterNumber : 0;

    public PhaseType GetCurrentPhaseType()
        => _currentPhase != null ? _currentPhase.phaseType : PhaseType.Day;

    public DayData GetCurrentDayData() => _currentPhase;

    public void SaveGame()
    {
        _playerDataSaver.SaveCharacterData();
        _worldSaveGameManager.SaveGame(GetCurrentChapterNumber(), GetCurrentPhaseType());
    }

    public void PauseMainGame(bool fadeToBlack = false)
    {
        Time.timeScale = 0f;

        SaveGame();
        _playerController.EnterCutsceneMode();

        InteractionPromptManager.Instance.HideComment();
        InteractionPromptManager.Instance.HideHint();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeMainGame(bool fadeFromBlack = true)
    {
        Time.timeScale = 1f;

        SaveGame();
        _playerController.ExitCutsceneMode();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (fadeFromBlack)
        {
            ScreenFaderTransition.Instance.FadeFromBlack();
        }
    }
}
