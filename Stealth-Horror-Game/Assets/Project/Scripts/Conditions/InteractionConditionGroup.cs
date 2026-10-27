using System.Collections.Generic;
using UnityEngine;

public enum GroupMode { All, Any }

[System.Serializable]
public class InteractionConditionGroup
{
    [Tooltip("All = все условия должны быть выполнены. Any = достаточно одного.")]
    public GroupMode mode = GroupMode.All;

    public List<InteractionCondition> conditions = new List<InteractionCondition>();
}