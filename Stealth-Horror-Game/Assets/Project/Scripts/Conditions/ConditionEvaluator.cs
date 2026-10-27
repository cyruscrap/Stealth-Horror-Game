using Zenject;

public class ConditionEvaluator
{
    private readonly WorldSaveGameManager _save;
    private readonly GameScenario _scenario;

    public ConditionEvaluator(WorldSaveGameManager save, GameScenario scenario)
    {
        _save = save;
        _scenario = scenario;
    }

    public bool AllGroupsMet(System.Collections.Generic.List<InteractionConditionGroup> groups)
    {
        if (groups == null || groups.Count == 0) return true;
        foreach (var g in groups)
            if (!GroupMet(g)) return false;
        return true;
    }

    private bool GroupMet(InteractionConditionGroup g)
    {
        if (g == null || g.conditions == null || g.conditions.Count == 0) return true;

        if (g.mode == GroupMode.All)
        {
            foreach (var c in g.conditions)
                if (!IsMet(c)) return false;
            return true;
        }
        else
        {
            foreach (var c in g.conditions)
                if (IsMet(c)) return true;
            return false;
        }
    }

    private bool IsMet(InteractionCondition c)
    {
        if (c == null) return false;
        var data = _save.currentCharacterSaveData;

        switch (c.type)
        {
            case ConditionType.ZoneVisited:
                foreach (var id in c.stringParams)
                    if (data.visitedZones.Contains(id)) return true;
                return false;

            case ConditionType.InteractedWithAny:
                foreach (var id in c.stringParams)
                    if (data.interactedObjects.Contains(id)) return true;
                return false;

            case ConditionType.ChapterNumber:
                return _scenario.GetCurrentChapterNumber() == c.intParam;

            case ConditionType.PhaseType:
                return _scenario.GetCurrentPhaseType() == c.phaseParam;

            case ConditionType.CutscenePlayed:
                foreach (var id in c.stringParams)
                    if (data.playedCutscenes.Contains(id)) return true;
                return false;

            case ConditionType.HasTriggeredEvent:
                foreach (var id in c.stringParams)
                    if (data.triggeredEvents.Contains(id)) return true;
                return false;

            default:
                return false;
        }
    }
}