using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public struct SubtitleText
{
    public float time;
    public string textKey;
    public string audioClipKey;
}

[CreateAssetMenu(fileName = "CutsceneData", menuName = "StealthHorrorGame/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    public string cutsceneID;
    public TimelineAsset timelineAsset;
    public string textKeyAfterCutscene;
    public SubtitleText[] subtitleText;
}
