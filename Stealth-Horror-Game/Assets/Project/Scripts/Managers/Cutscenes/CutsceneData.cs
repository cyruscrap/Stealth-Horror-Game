using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public struct SubtitleText
{
    public float time;
    public string text;
}

[CreateAssetMenu(fileName = "CutsceneData", menuName = "StealthHorrorGame/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    public TimelineAsset timelineAsset;
    public SubtitleText[] subtitleText;
}
