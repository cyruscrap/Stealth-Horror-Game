using UnityEngine;
using Zenject;

public class CutscenePlayedCondition : InteractionConditionBase
{
    [SerializeField] private string _cutsceneID;

    [Inject] private CutsceneManager _cutsceneManager;

    public override bool IsMet()
    {
        return _cutsceneManager != null && _cutsceneManager.HasCutsceneBeenPlayed(_cutsceneID);
    }
}
