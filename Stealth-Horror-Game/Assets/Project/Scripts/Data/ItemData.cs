using System.Collections.Generic;
using UnityEngine;

public enum ItemTag { Other, Diary, Phone, Stove }

[System.Serializable]
public class PhaseEntries
{
    public DayData phase;
    public List<InteractionEntry> entries = new List<InteractionEntry>();
}

[CreateAssetMenu(fileName = "Item_", menuName = "StealthHorrorGame/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public ItemTag tag;
    public string objectID;
    public GameObject prefab;

    [Header("Interactions by phase")]
    [Tooltip("ƒл€ каждой фазы Ч свой список записей. ≈сли текущей фазы тут нет Ч объект бездействует.")]
    public List<PhaseEntries> phaseEntries = new List<PhaseEntries>();

    public List<InteractionEntry> GetEntriesFor(DayData phase)
    {
        foreach (var pe in phaseEntries)
            if (pe != null && pe.phase == phase) return pe.entries;
        return null;
    }
}