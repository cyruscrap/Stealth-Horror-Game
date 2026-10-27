using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class ConditionTrigger : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("”никальный ID триггера. ≈сли пусто Ч используетс€ им€ GameObject.")]
    [SerializeField] private string triggerID;

    [Header("Conditions (data)")]
    [Tooltip("ћежду группами Ч AND. ¬нутри группы Ч All или Any.")]
    [SerializeField] private List<InteractionConditionGroup> conditionGroups = new List<InteractionConditionGroup>();

    [Header("Flags")]
    [SerializeField] private bool oneShot = true;

    [Header("Scene action")]
    [SerializeField] private UnityEvent onConditionsMet;

    [Header("System actions (optional)")]
    [Tooltip("≈сли не пусто Ч SetUniversalState(key) на GlobalInteractionState.")]
    [SerializeField] private string setUniversalCommentKey;

    [Tooltip("≈сли true Ч сбросить UniversalInteract на GlobalInteractionState.")]
    [SerializeField] private bool resetUniversalState;

    [Tooltip("≈сли true Ч вызвать GameScenario.AdvanceToNextPhase() после срабатывани€.")]
    [SerializeField] private bool advanceToNextPhase;

    [Inject] private WorldSaveGameManager _saveManager;
    [Inject] private GameScenario _gameScenario;
    [Inject] private ConditionEvaluator _evaluator;
    [Inject] private GlobalInteractionState _globalState;

    public string EffectiveId => string.IsNullOrEmpty(triggerID) ? gameObject.name : triggerID;

    private void OnEnable()
    {
        _saveManager.OnStateChanged += Check;
        Check(); // проверить сразу при по€влении объекта
    }

    private void OnDisable()
    {
        _saveManager.OnStateChanged -= Check;
    }

    private void Check()
    {
        var data = _saveManager.currentCharacterSaveData;

        // ”же срабатывал и одноразовый Ч пропускаем
        if (oneShot && data.triggeredEvents.Contains(EffectiveId)) return;

        // ѕровер€ем услови€ через общий оценщик
        if (!_evaluator.AllGroupsMet(conditionGroups)) return;

        // ѕомечаем до вызова действий, чтобы повторный Check не запустил их ещЄ раз
        if (oneShot)
        {
            data.triggeredEvents.Add(EffectiveId);
        }

        // 1) —ценовое действие (UnityEvent)
        onConditionsMet?.Invoke();

        // 2) —истемные действи€ Ч через сервисы
        if (!string.IsNullOrEmpty(setUniversalCommentKey))
            _globalState.SetUniversalState(setUniversalCommentKey);

        if (resetUniversalState)
            _globalState.Reset();

        // 3) —охран€ем состо€ние
        _gameScenario.SaveGame();

        // 4) ѕереход фазы Ч в самом конце
        if (advanceToNextPhase)
            _gameScenario.AdvanceToNextPhase();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(triggerID))
            Debug.LogWarning($"[ConditionTrigger] '{name}': triggerID пуст, используетс€ им€ объекта. " +
                             $"≈сли переименуете объект Ч сейв потер€ет св€зь.", this);

        if (conditionGroups == null || conditionGroups.Count == 0)
            Debug.LogWarning($"[ConditionTrigger] '{name}': нет условий. “риггер сработает сразу.", this);
    }
#endif
}