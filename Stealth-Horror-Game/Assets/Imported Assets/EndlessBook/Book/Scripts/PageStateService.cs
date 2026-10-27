using System.Collections.Generic;
using UnityEngine;

public interface IPageStateService
{
    bool IsPageSeen(int pageNumber);
    void MarkPageSeen(int pageNumber);
    void Save();
}

public class PageStateService : IPageStateService
{
    private const string PrefsKey = "diary_seen_pages";
    private readonly HashSet<int> _seen = new HashSet<int>();

    public PageStateService()
    {
        Load();
    }

    public bool IsPageSeen(int pageNumber) => _seen.Contains(pageNumber);

    public void MarkPageSeen(int pageNumber)
    {
        if (_seen.Add(pageNumber))
        {
            Save();
        }
    }

    public void Save()
    {
        // Простейший формат: "1,4,7,12"
        PlayerPrefs.SetString(PrefsKey, string.Join(",", _seen));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        var raw = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(raw)) return;
        foreach (var s in raw.Split(','))
        {
            if (int.TryParse(s, out var n)) _seen.Add(n);
        }
    }
}