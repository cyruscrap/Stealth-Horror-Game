using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionEntry
{
    [Tooltip("Уникальный ID записи. Обязателен, если используется oneShot или сценовое действие по entryId.")]
    public string entryID;

    [Header("Action data")]
    [Tooltip("Ключ таблицы I2 Localization")]
    public string commentTextKey;
    [Tooltip("Если не пусто — все реплики из списка проигрываются подряд за одно взаимодействие.")]
    public List<string> commentTextSequence = new List<string>();
    public string miniGameScene;
    public string conversation;
    public string animationTrigger;
    public CutsceneData cutscene;
    public AudioClip audioClip;

    [Header("System actions")]
    [Tooltip("Если не пусто — устанавливает UniversalCommentKey и включает UniversalInteract.")]
    public string setUniversalCommentKey;

    [Tooltip("Если true — сбрасывает UniversalInteract.")]
    public bool resetUniversalState;

    [Tooltip("Если true — после взаимодействия вызывается AdvanceToNextPhase.")]
    public bool advanceToNextPhase;

    [Header("Flags")]
    public int priority = 0;
    public bool oneShot = true;

    [Header("Conditions (data)")]
    public List<InteractionConditionGroup> conditionGroups = new List<InteractionConditionGroup>();
}