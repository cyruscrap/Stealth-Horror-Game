using UnityEngine;
using Zenject;

public class LampController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Источники света, которые нужно включать/выключать")]
    [SerializeField] private Light[] lights;

    [Tooltip("Рендереры (стекла, плафоны), у которых есть эмиссия")]
    [SerializeField] private Renderer[] emissiveRenderers;

    [Header("Emission Settings")]
    [SerializeField] private Color emissionColor = Color.white;
    [SerializeField] private float emissionIntensity = 1f;
    [Tooltip("Имя свойства эмиссии в шейдере (для URP/HDRP обычно _EmissionColor)")]
    [SerializeField] private string emissionPropertyName = "_EmissionColor";

    [Header("Phase Settings")]
    [SerializeField] private bool turnOnDuringDay = true;
    [SerializeField] private bool turnOnDuringNight = false;

    [Inject] private EventManager _eventManager;

    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        _eventManager.OnPhaseStart += ApplyPhaseState;
    }

    private void OnDisable()
    {
        _eventManager.OnPhaseStart -= ApplyPhaseState;
    }

    private void Start()
    {
        // Применяем состояние сразу при старте сцены, если фаза уже задана
        // (GameScenario вызовет OnPhaseStart чуть позже, но для надежности можно дернуть и тут)
        if (GameScenario.Instance != null)
        {
            ApplyPhaseState(GameScenario.Instance.GetCurrentPhase());
        }
    }

    private void ApplyPhaseState(DayData phase)
    {
        if (phase == null) return;

        bool shouldBeOn = (phase.phaseType == PhaseType.Day && turnOnDuringDay) ||
                          (phase.phaseType == PhaseType.Night && turnOnDuringNight);

        // 1. Включаем/выключаем физический свет
        if (lights != null)
        {
            foreach (var lightComp in lights)
            {
                if (lightComp != null) lightComp.enabled = shouldBeOn;
            }
        }

        // 2. Включаем/выключаем свечение (эмиссию) на материалах
        if (emissiveRenderers != null)
        {
            Color finalEmission = shouldBeOn ? (emissionColor * emissionIntensity) : Color.black;

            foreach (var rendererComp in emissiveRenderers)
            {
                if (rendererComp == null) continue;

                // Используем MaterialPropertyBlock, чтобы не создавать копии материалов
                rendererComp.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(emissionPropertyName, finalEmission);
                rendererComp.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}