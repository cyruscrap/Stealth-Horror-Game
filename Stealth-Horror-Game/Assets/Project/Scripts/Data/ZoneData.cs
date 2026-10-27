using System.Collections.Generic;
using UnityEngine;

public enum ZoneEventType { OnEnter, OnExit, OnFirstEnter }

[System.Serializable]
public class PhaseZoneEntries
{
    public DayData phase;
    public List<CommentEntry> entries = new List<CommentEntry>();
}

[CreateAssetMenu(fileName = "Zone_", menuName = "StealthHorrorGame/ZoneData")]
public class ZoneData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Уникальный ID зоны. Попадает в visitedZones при входе игрока.")]
    public string zoneID;

    [Tooltip("Когда срабатывает зона: при входе, выходе или только при первом входе.")]
    public ZoneEventType eventType = ZoneEventType.OnFirstEnter;

    [Header("Entries by phase")]
    [Tooltip("Для каждой фазы — свой список комментариев. Если текущей фазы нет — зона молчит.")]
    public List<PhaseZoneEntries> phaseEntries = new List<PhaseZoneEntries>();

    public List<CommentEntry> GetEntriesFor(DayData phase)
    {
        foreach (var pe in phaseEntries)
            if (pe != null && pe.phase == phase) return pe.entries;
        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(zoneID))
            Debug.LogWarning($"[ZoneData] '{name}': zoneID пуст. Зона не будет попадать в visitedZones.", this);

        foreach (var pe in phaseEntries)
        {
            if (pe == null || pe.entries == null) continue;
            for (int i = 0; i < pe.entries.Count; i++)
            {
                var e = pe.entries[i];
                if (e == null) continue;
                if (string.IsNullOrEmpty(e.commentID))
                    Debug.LogWarning($"[ZoneData] '{name}', phase {(pe.phase ? pe.phase.name : "null")}, entry {i}: entryId пуст. " +
                                     $"Если нужен onShow в сцене, entryId обязателен.", this);
            }
        }
    }
#endif
}