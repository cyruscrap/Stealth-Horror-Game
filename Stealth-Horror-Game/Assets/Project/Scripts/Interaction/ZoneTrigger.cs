using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class ZoneEntryAction
{
    [Tooltip("ƒолжен совпадать с entryId в ZoneData")]
    public string entryId;
    public UnityEvent onShow;
}

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private ZoneData zoneData;

    [Tooltip("Ћокальные действи€ в сцене, прив€занные к запис€м по entryId")]
    [SerializeField] private List<ZoneEntryAction> actions = new List<ZoneEntryAction>();

    [Inject] private GameScenario _gameScenario;
    [Inject] private ConditionEvaluator _evaluator;
    [Inject] private WorldSaveGameManager _saveManager;
    [Inject] private EventManager _eventManager;

    private bool _playerInside;

    public bool HasBeenVisited
    {
        get
        {
            if (zoneData == null || string.IsNullOrEmpty(zoneData.zoneID)) return false;
            return _saveManager.currentCharacterSaveData.visitedZones.Contains(zoneData.zoneID);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_playerInside) return; // защита от повторных срабатываний на нескольких коллайдерах
        _playerInside = true;

        if (zoneData == null)
        {
            Debug.LogWarning($"[ZoneTrigger] {name}: zoneData не назначен");
            return;
        }

        bool wasVisited = HasBeenVisited;
        MarkVisited();

        switch (zoneData.eventType)
        {
            case ZoneEventType.OnEnter:
                Evaluate(_gameScenario.GetCurrentPhase());
                break;

            case ZoneEventType.OnFirstEnter:
                if (!wasVisited)
                    Evaluate(_gameScenario.GetCurrentPhase());
                break;

            case ZoneEventType.OnExit:
                // обрабатываетс€ в OnTriggerExit
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;

        if (zoneData == null) return;
        if (zoneData.eventType != ZoneEventType.OnExit) return;

        Evaluate(_gameScenario.GetCurrentPhase());
    }

    private void MarkVisited()
    {
        if (zoneData == null || string.IsNullOrEmpty(zoneData.zoneID)) return;

        var visited = _saveManager.currentCharacterSaveData.visitedZones;
        if (!visited.Contains(zoneData.zoneID))
        {
            visited.Add(zoneData.zoneID);
            _saveManager.NotifyStateChanged();
        }
    }

    private void Evaluate(DayData currentPhase)
    {
        var entries = zoneData.GetEntriesFor(currentPhase);
        if (entries == null || entries.Count == 0) return;

        CommentEntry bestEntry = null;
        int bestPriority = int.MinValue;

        foreach (var entry in entries)
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

        if (bestEntry == null) return;

        string effectiveId = GetEffectiveId(bestEntry);

        // 1) ѕометить одноразовую запись как показанную
        if (bestEntry.oneShot)
        {
            var shown = _saveManager.currentCharacterSaveData.shownComments;
            if (!shown.Contains(effectiveId))
            {
                shown.Add(effectiveId);
                _saveManager.NotifyStateChanged();
            }
        }

        // 2) Ћокальное действие в сцене
        foreach (var a in actions)
        {
            if (a.entryId == effectiveId)
            {
                a.onShow?.Invoke();
                break;
            }
        }

        // 3) ѕриоритет отдаЄм последовательности, если она задана
        if (bestEntry.commentTextSequence != null && bestEntry.commentTextSequence.Count > 0)
            InteractionPromptManager.Instance.ShowCommentSequence(bestEntry.commentTextSequence);
        else if (!string.IsNullOrEmpty(bestEntry.textKey))
            InteractionPromptManager.Instance.ShowComment(bestEntry.textKey);

        if (bestEntry.audioClip != null) AudioSource.PlayClipAtPoint(bestEntry.audioClip, transform.position);
    }

    private bool IsAlreadyShown(CommentEntry entry)
    {
        if (!entry.oneShot) return false;
        string id = GetEffectiveId(entry);
        return _saveManager.currentCharacterSaveData.shownComments.Contains(id);
    }

    private string GetEffectiveId(CommentEntry entry)
    {
        if (!string.IsNullOrEmpty(entry.commentID)) return entry.commentID;

        // Fallback, если entryId забыли заполнить: zoneID + индекс
        var entries = zoneData.GetEntriesFor(_gameScenario.GetCurrentPhase());
        int idx = entries != null ? entries.IndexOf(entry) : -1;
        return $"{zoneData.zoneID}_{idx}";
    }
}