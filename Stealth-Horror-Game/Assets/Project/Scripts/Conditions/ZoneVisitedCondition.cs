using UnityEngine;

public class ZoneVisitedCondition : InteractionConditionBase
{
    [SerializeField] private ZoneTrigger _zoneTrigger;

    private void Awake()
    {
        if (_zoneTrigger == null)
        {
            _zoneTrigger = GetComponent<ZoneTrigger>();
            if (_zoneTrigger == null) Debug.LogError("ZoneTrigger reference is not set in ZoneVisitedCondition.");
        }
    }

    public override bool IsMet()
    {
        return _zoneTrigger != null && _zoneTrigger.HasBeenVisited;
    }
}
