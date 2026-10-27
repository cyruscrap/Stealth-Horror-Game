using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CommentEntry
{
    [Tooltip("Уникальный ID записи. Обязателен, если нужен onShow в сцене " +
             "или если две записи должны различаться по oneShot.")]
    public string commentID;

    [Header("Content")]
    [Tooltip("Ключ таблицы I2 Localization")]
    public string textKey;
    [Tooltip("Если не пусто — все реплики из списка проигрываются подряд за одно взаимодействие.")]
    public List<string> commentTextSequence = new List<string>();
    public AudioClip audioClip;

    [Header("Flags")]
    [Tooltip("Показывать только один раз за сохранение")]
    public bool oneShot = true;
    [Tooltip("Чем больше число, тем выше приоритет")]
    public int priority = 0;

    [Header("External Conditions")]
    [Tooltip("Ссылки на компоненты, реализующие IInteractionCondition")]
    public List<InteractionConditionGroup> conditionGroups = new List<InteractionConditionGroup>();

    public string GetEffectiveId(string zoneId, int index)
    {
        if (!string.IsNullOrEmpty(commentID)) return commentID;
        return $"{zoneId}_{index}";
    }
}