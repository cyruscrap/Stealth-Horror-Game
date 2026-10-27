using System.Collections.Generic;

public class GlobalInteractionState
{
    public bool UniversalInteract { get; private set; }
    public string UniversalCommentKey { get; private set; }

    private readonly Dictionary<string, string> _triggers = new();
    private readonly HashSet<string> _exempt = new();

    public void RegisterExempt(string objectId) => _exempt.Add(objectId);

    public bool IsBlocked(string objectId) => UniversalInteract && !_exempt.Contains(objectId);

    public void SetUniversalState(string commentTextKey)
    {
        UniversalInteract = true;
        UniversalCommentKey = commentTextKey;
    }

    public void Reset()
    {
        UniversalInteract = false;
        UniversalCommentKey = null;
    }
}