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

    [Tooltip("Список ID: для ZoneVisited — одна зона, для InteractedWithAny — несколько")]
    public List<string> stringParams = new List<string>();

    [Tooltip("Если 0, глава не проверяется")]
    public int intParam = 0;         // для ChapterNumber
    [Tooltip("Если Any, фаза не проверяется")]
    public PhaseType phaseParam = PhaseType.Any; // для PhaseType
}