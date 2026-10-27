using UnityEngine;

public static class PlayerPrefsExtentions
{
    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        int defaultInt = defaultValue ? 1 : 0;
        return PlayerPrefs.GetInt(key, defaultInt) == 1;
    }
}
