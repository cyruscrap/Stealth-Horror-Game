using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityTutorial.PlayerControl;
using Zenject;

[System.Serializable]
public class ItemEntryAction
{
    [Tooltip("Должен совпадать с entryID в ItemData")]
    public string entryID;
    public UnityEvent onInteract;
}

/// <summary>
/// Компонент для любого предмета, с которым можно взаимодействовать.
/// Данные (условия, тексты, действия) — в ItemData.
/// Локальные сценовые действия — в actions по entryId.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    [SerializeField] private string objectID;
    [SerializeField] private bool exemptFromUniversalComment;

    [Header("Data")]
    [SerializeField] private ItemData itemData;

    [Tooltip("Локальные действия в сцене, привязанные к записям по entryId")]
    [SerializeField] private List<ItemEntryAction> actions = new List<ItemEntryAction>();

    private Outline _outline;
    private Animator _animator;
    private Collider _interactableCollider;
    private AudioSource _audioSource;

    private GameScenario _gameScenario;
    private PlayerController _playerController;
    private EventManager _eventManager;
    private CutsceneManager _cutsceneManager;
    private WorldSaveGameManager _saveManager;
    private GlobalInteractionState _globalState;
    private ConditionEvaluator _evaluator;

    private InteractionEntry _currentInteractionEntry;
    private Vector3 _interactableCenter;

    private bool _canBeInteracted = true;
    private bool _hasBeenInteracted = false;

    public bool HasBeenInteracted => _hasBeenInteracted;
    public string EffectiveObjectId => string.IsNullOrEmpty(objectID) ? gameObject.name : objectID;

    [Inject]
    public void Construct(GameScenario gameScenario,
                          PlayerController playerController,
                          EventManager eventManager,
                          CutsceneManager cutsceneManager,
                          WorldSaveGameManager saveManager,
                          GlobalInteractionState globalState,
                          ConditionEvaluator evaluator)
    {
        _gameScenario = gameScenario;
        _playerController = playerController;
        _eventManager = eventManager;
        _cutsceneManager = cutsceneManager;
        _saveManager = saveManager;
        _globalState = globalState;
        _evaluator = evaluator;
    }

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (_interactableCollider == null)
            _interactableCollider = GetComponent<Collider>();

        if (exemptFromUniversalComment)
            _globalState.RegisterExempt(EffectiveObjectId);

        if (_interactableCollider != null)
            _interactableCenter = _interactableCollider.bounds.center;
        else
            _interactableCenter = transform.position;

        if (_outline != null)
            _outline.enabled = false;
    }

    private void Start()
    {

    }

    // --- Фокус ---

    public void OnFocus(bool showOutline)
    {
        InteractionPromptManager.Instance.ShowHint(_interactableCenter);
        if (_outline != null && showOutline)
            _outline.enabled = true;
    }

    public void OnLoseFocus()
    {
        InteractionPromptManager.Instance.HideHint();
        if (_outline != null && _outline.enabled)
            _outline.enabled = false;
    }

    // --- Флаги ---

    public bool CanBeInteracted() => _canBeInteracted;

    public void SetCanBeInteracted(bool value) => _canBeInteracted = value;

    private IEnumerator SwitchCanBeInteracted(float delay)
    {
        SetCanBeInteracted(false);
        yield return new WaitForSeconds(delay);
        SetCanBeInteracted(true);
    }

    private void ResetFlags()
    {
        StopAllCoroutines();
        SetCanBeInteracted(false);
    }

    // --- Взаимодействие ---

    public void Interact()
    {
        if (!_canBeInteracted)
        {
            Debug.LogWarning("CanBeInteracted disabled!");
            return;
        }

        if (_globalState.IsBlocked(EffectiveObjectId))
        {
            if (!string.IsNullOrEmpty(_globalState.UniversalCommentKey))
                InteractionPromptManager.Instance.ShowComment(_globalState.UniversalCommentKey);
            return;
        }

        if (itemData == null)
        {
            Debug.LogWarning($"[InteractableItem] {name}: itemData не назначен");
            return;
        }

        DayData currentPhase = _gameScenario.GetCurrentPhase();
        var entries = itemData.GetEntriesFor(currentPhase);
        if (entries == null || entries.Count == 0)
        {
            Debug.Log($"[InteractableItem] {name}: нет записей для фазы {currentPhase?.phaseID}");
            return;
        }

        // Выбор записи: максимальный приоритет среди тех, где условия выполнены
        // и которые ещё не помечены как показанные (для oneShot)
        InteractionEntry bestEntry = null;
        int bestPriority = int.MinValue;

        foreach (InteractionEntry entry in entries)
        {
            if (entry == null) continue;
            if (!_evaluator.AllGroupsMet(entry.conditionGroups)) continue;
            if (IsAlreadyShown(entry)) continue;

            if (entry.priority > bestPriority)
            {
                bestPriority = entry.priority;
                bestEntry = entry;
            }
        }

        if (bestEntry == null)
        {
            Debug.Log("No matching interaction entry.");
            OnLoseFocus();
            return;
        }

        _currentInteractionEntry = bestEntry;

        // Запоминаем факт взаимодействия с объектом
        CharacterSaveData saveData = _saveManager.currentCharacterSaveData;
        if (!saveData.interactedObjects.Contains(EffectiveObjectId))
        {
            saveData.interactedObjects.Add(EffectiveObjectId);
            _gameScenario.SaveGame();
        }
        _saveManager.NotifyStateChanged();

        _hasBeenInteracted = true;

        if (_audioSource != null)
            _audioSource.Play();

        OnLoseFocus();

        // Помечаем одноразовую запись как показанную
        string effectiveId = GetEffectiveID(bestEntry);
        if (bestEntry.oneShot)
        {
            if (!_saveManager.currentCharacterSaveData.shownItemInteractions.Contains(effectiveId))
            {
                _saveManager.currentCharacterSaveData.shownItemInteractions.Add(effectiveId);
                _gameScenario.SaveGame();
            }
        }

        // 1) Локальное сценовое действие по entryId (если есть)
        InvokeLocalAction(effectiveId);

        // 2) Общее действие из данных ItemData
        ExecuteEntry(bestEntry);
    }

    private void InvokeLocalAction(string entryId)
    {
        foreach (ItemEntryAction a in actions)
        {
            if (a != null && a.entryID == entryId)
            {
                a.onInteract?.Invoke();
                return;
            }
        }
    }

    private void ExecuteEntry(InteractionEntry entry)
    {
        if (entry.cutscene != null)
        {
            ScreenFaderTransition.Instance.FadeToBlack(null, LoadSceneMode.Single, () =>
            {
                ScreenFaderTransition.Instance.FadeFromBlack();
                _cutsceneManager.PlayNewCutscene(entry.cutscene);
            });
            return;
        }

        if (!string.IsNullOrEmpty(entry.conversation))
        {
            DialogueManager.StartConversation(entry.conversation, _playerController.transform, transform);
            _eventManager.ReportTalkingStarted();
            return;
        }

        if (!string.IsNullOrEmpty(entry.miniGameScene))
        {
            StartCoroutine(SwitchCanBeInteracted(5));
            DiaryLaunchPayload.CurrentDay = _gameScenario.GetCurrentDayData();
            ScreenFaderTransition.Instance.FadeToBlack(entry.miniGameScene, LoadSceneMode.Additive, () =>
            {
                _gameScenario.PauseMainGame();
            });
            return;
        }

        if (!string.IsNullOrEmpty(entry.animationTrigger))
        {
            if (_animator != null) _animator.SetTrigger(entry.animationTrigger);
            else Debug.LogWarning("No Animator component found for animation: " + entry.animationTrigger);
        }

        // Приоритет отдаём последовательности, если она задана
        if (entry.commentTextSequence != null && entry.commentTextSequence.Count > 0)
            InteractionPromptManager.Instance.ShowCommentSequence(entry.commentTextSequence);
        else if (!string.IsNullOrEmpty(entry.commentTextKey))
            InteractionPromptManager.Instance.ShowComment(entry.commentTextKey);

        if (entry.audioClip != null) AudioSource.PlayClipAtPoint(entry.audioClip, transform.position);

        // 3) Системные действия — вызываются через инжектированные сервисы
        if (!string.IsNullOrEmpty(entry.setUniversalCommentKey))
            _globalState.SetUniversalState(entry.setUniversalCommentKey);

        if (entry.resetUniversalState)
            _globalState.Reset();

        if (entry.advanceToNextPhase)
            _gameScenario.AdvanceToNextPhase();
    }

    // --- Условия и одноразовость ---

    private bool IsAlreadyShown(InteractionEntry entry)
    {
        if (!entry.oneShot) return false;
        string id = GetEffectiveID(entry);
        return _saveManager.currentCharacterSaveData.shownItemInteractions.Contains(id);
    }

    private string GetEffectiveID(InteractionEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.entryID)) return entry.entryID;

        // Fallback, если entryId забыли заполнить: objectID + индекс в текущей фазе
        var entries = itemData.GetEntriesFor(_gameScenario.GetCurrentPhase());
        int idx = entries != null ? entries.IndexOf(entry) : -1;
        return $"{EffectiveObjectId}_{idx}";
    }

    public void OnInteractionEnded()
    {
        if (_currentInteractionEntry != null && !string.IsNullOrEmpty(_currentInteractionEntry.conversation))
        {
            _eventManager.ReportTalkingEnded();
            _currentInteractionEntry = null;
        }
    }
}