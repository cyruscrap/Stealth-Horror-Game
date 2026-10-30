using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Task_", menuName = "StealthHorrorGame/TaskData")]
public class TaskData : ScriptableObject
{
    [Header("Identity")]
    public string taskID;

    [Tooltip("Ключ локализации I2 для текста задачи")]
    public string taskTextKey;

    [Header("Conditions")]
    [Tooltip("Условия, при которых задача появится на экране.")]
    public List<InteractionConditionGroup> showConditions = new List<InteractionConditionGroup>();

    [Tooltip("Условия, при которых задача исчезнет с экрана (считается выполненной).")]
    public List<InteractionConditionGroup> completeConditions = new List<InteractionConditionGroup>();
}