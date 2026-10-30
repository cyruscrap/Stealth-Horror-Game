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

        bool typeMet = false;

        // 1. ѕровер€ем основной тип услови€
        switch (c.type)
        {
            case ConditionType.ZoneVisited:
                foreach (var id in c.stringParams)
                    if (data.visitedZones.Contains(id)) { typeMet = true; break; }
                break;

            case ConditionType.InteractedWithAny:
                foreach (var id in c.stringParams)
                    if (data.interactedObjects.Contains(id)) { typeMet = true; break; }
                break;

            case ConditionType.HasTriggeredEvent:
                foreach (var id in c.stringParams)
                    if (data.triggeredEvents.Contains(id)) { typeMet = true; break; }
                break;

            case ConditionType.CutscenePlayed:
                foreach (var id in c.stringParams)
                    if (data.playedCutscenes.Contains(id)) { typeMet = true; break; }
                break;

            case ConditionType.ChapterNumber:
                // ≈сли это €вное условие на главу, провер€ем сразу и выходим
                return _scenario.GetCurrentChapterNumber() == c.intParam;

            case ConditionType.PhaseType:
                // ≈сли это €вное условие на фазу, провер€ем сразу и выходим
                return _scenario.GetCurrentPhaseType() == c.phaseParam;
        }

        if (!typeMet) return false; // ≈сли тип не совпал (например, зона не посещена) - дальше не идем

        // 2. ѕровер€ем дополнительную главу (если intParam > 0)
        if (c.intParam > 0)
        {
            if (_scenario.GetCurrentChapterNumber() != c.intParam) return false;
        }

        // 3. ѕровер€ем дополнительную фазу (если phaseParam != Any)
        if (c.phaseParam != PhaseType.Any)
        {
            if (_scenario.GetCurrentPhaseType() != c.phaseParam) return false;
        }

        return true;
    }
}