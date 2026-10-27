using System.Collections.Generic;
using UnityEngine;

public enum ConditionType
{
    ZoneVisited,
    InteractedWithAny,
    ChapterNumber,
    PhaseType,
    HasTriggeredEvent,
    CutscenePlayed
}

[System.Serializable]
public class InteractionCondition
{
    public ConditionType type;

    [Tooltip("—писок ID: дл€ ZoneVisited Ч одна зона, дл€ InteractedWithAny Ч несколько")]
    public List<string> stringParams = new List<string>();

    public int intParam;         // дл€ ChapterNumber
    public PhaseType phaseParam; // дл€ PhaseType
}