using UnityEngine;

[CreateAssetMenu(fileName = "LightingPreset_", menuName = "StealthHorrorGame/LightingPreset")]
public class LightingPreset : ScriptableObject
{
    [Header("Skybox")]
    public Material skybox;
    [Range(0f, 2f)] public float skyboxExposure = 1f;
    [Tooltip("Если включено — при смене skybox пересчитывается ambient")]
    public bool updateAmbientFromSkybox = true;
    [Tooltip("Задержка перед пересчётом ambient (сек), чтобы Unity успела обновить отражения")]
    public float ambientUpdateDelay = 0.1f;

    [Header("Ambient")]
    public Color ambientColor = Color.gray;
    public float ambientIntensity = 1f;

    [Header("Sun / Moon")]
    public Color sunColor = Color.white;
    public float sunIntensity = 1f;
    public Vector3 sunEulerAngles = new Vector3(50f, -30f, 0f);

    [Header("Fog")]
    public bool fogEnabled;
    public Color fogColor = Color.black;
    public float fogDensity = 0.01f;
}