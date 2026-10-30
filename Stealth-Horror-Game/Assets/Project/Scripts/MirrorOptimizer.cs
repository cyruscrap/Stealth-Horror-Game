using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(MeshRenderer))]
public class MirrorOptimizer : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Camera mirrorCamera;

    [Header("Настройки оптимизации")]
    [Tooltip("Отключать ли тени для камеры зеркала?")]
    [SerializeField] private bool disableShadowsForMirror = true;

    [Tooltip("Как часто проверять угол обзора игрока (в секундах). Чем больше, тем выше FPS.")]
    [SerializeField] private float checkInterval = 0.1f;

    private MeshRenderer _meshRenderer;
    private Transform _playerTransform;
    private Coroutine _angleCheckCoroutine;
    private bool _isMeshVisible;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();

        if (Camera.main != null)
        {
            _playerTransform = Camera.main.transform;
        }

        if (mirrorCamera == null)
        {
            mirrorCamera = GetComponentInChildren<Camera>();
        }

        ConfigureMirrorCamera();

        // Инициализируем стартовое состояние
        _isMeshVisible = _meshRenderer.isVisible;
        ToggleMirrorState();
    }

    private void ConfigureMirrorCamera()
    {
        if (mirrorCamera == null) return;

        var cameraData = mirrorCamera.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData != null)
        {
            cameraData.renderShadows = !disableShadowsForMirror;
        }
    }

    private void OnBecameVisible()
    {
        _isMeshVisible = true;
        ToggleMirrorState();
    }

    private void OnBecameInvisible()
    {
        _isMeshVisible = false;
        ToggleMirrorState();
    }

    private void ToggleMirrorState()
    {
        if (_isMeshVisible)
        {
            // Включаем проверку угла, только если зеркало в поле видимости
            if (_angleCheckCoroutine == null && _playerTransform != null)
            {
                _angleCheckCoroutine = StartCoroutine(CheckPlayerAngleRoutine());
            }
        }
        else
        {
            // Если зеркало ушло из видимости, полностью тушим камеру и сопрограмму
            if (_angleCheckCoroutine != null)
            {
                StopCoroutine(_angleCheckCoroutine);
                _angleCheckCoroutine = null;
            }
            if (mirrorCamera != null)
            {
                mirrorCamera.enabled = false;
            }
        }
    }

    // Оптимизированный цикл вместо Update
    private IEnumerator<WaitForSeconds> CheckPlayerAngleRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);
        Transform mirrorTransform = transform;
        Vector3 mirrorForward = mirrorTransform.forward;

        while (true)
        {
            Vector3 toPlayer = _playerTransform.position - mirrorTransform.position;

            // Оптимизация: используем Vector3.Dot без нормализации вектора.
            // Нам важен только знак результата (плюс или минус), 
            // поэтому точная длина вектора (корень) не нужна.
            float dotProduct = Vector3.Dot(mirrorForward, toPlayer);

            // Если игрок перед зеркалом (dotProduct > 0), включаем камеру. 
            // Если за ним (повернулся/ушел за стену в той же комнате) — выключаем.
            if (mirrorCamera != null)
            {
                mirrorCamera.enabled = (dotProduct > 0);
            }

            yield return wait;
        }
    }

    private void OnDisable()
    {
        // Защита от утечек памяти при уничтожении объекта
        if (_angleCheckCoroutine != null)
        {
            StopCoroutine(_angleCheckCoroutine);
            _angleCheckCoroutine = null;
        }
    }
}
