using UnityEngine;
using Zenject;

public class PhaseTypeCondition : InteractionConditionBase
{
    [SerializeField] private PhaseType requiredType;
    [Inject] private GameScenario _gameScenario;

    public override bool IsMet()
        => _gameScenario.GetCurrentPhaseType() == requiredType;
}