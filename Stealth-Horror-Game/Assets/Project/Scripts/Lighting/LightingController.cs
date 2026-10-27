using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class LightingController : MonoBehaviour
{
    [SerializeField] private Light sunLight;

    [Inject] private EventManager _eventManager;

    private Coroutine _ambientRoutine;

    private void OnEnable() => _eventManager.OnPhaseStart += ApplyPreset;
    private void OnDisable() => _eventManager.OnPhaseStart -= ApplyPreset;

    private void ApplyPreset(DayData phase)
    {
        var p = phase != null ? phase.lightingPreset : null;
        if (p == null) return;

        // Skybox
        if (p.skybox != null)
        {
            RenderSettings.skybox = p.skybox;
            RenderSettings.skybox.SetFloat("_Exposure", p.skyboxExposure);
        }

        // Ambient
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = p.ambientColor;
        RenderSettings.ambientIntensity = p.ambientIntensity;

        // Fog
        RenderSettings.fog = p.fogEnabled;
        RenderSettings.fogColor = p.fogColor;
        RenderSettings.fogDensity = p.fogDensity;

        // Sun / Moon
        if (sunLight != null)
        {
            sunLight.color = p.sunColor;
            sunLight.intensity = p.sunIntensity;
            sunLight.transform.rotation = Quaternion.Euler(p.sunEulerAngles);
        }

        // Динамические объекты должны получить обновлённое освещение
        DynamicGI.UpdateEnvironment();

        // Если нужно — пересчитать ambient из нового skybox
        if (p.updateAmbientFromSkybox)
        {
            if (_ambientRoutine != null) StopCoroutine(_ambientRoutine);
            _ambientRoutine = StartCoroutine(UpdateAmbientFromSkybox(p.ambientUpdateDelay));
        }
    }

    private IEnumerator UpdateAmbientFromSkybox(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        DynamicGI.UpdateEnvironment();
        _ambientRoutine = null;
    }
}