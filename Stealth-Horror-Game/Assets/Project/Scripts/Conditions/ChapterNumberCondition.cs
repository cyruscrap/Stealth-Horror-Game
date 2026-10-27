using UnityEngine;
using Zenject;

public class ChapterNumberCondition : InteractionConditionBase
{
    [SerializeField] private int requiredChapter;
    [Inject] private GameScenario _gameScenario;

    public override bool IsMet()
        => _gameScenario.GetCurrentChapterNumber() == requiredChapter;
}