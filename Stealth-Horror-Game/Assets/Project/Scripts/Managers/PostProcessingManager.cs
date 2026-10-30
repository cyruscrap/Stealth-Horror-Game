using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager
{
    private readonly Volume _globalVolume;
    private LiftGammaGain _liftGammaGain;
    private MotionBlur _motionBlur;

    public PostProcessingManager(Volume globalVolume)
    {
        if (globalVolume == null)
        {
            Debug.LogError("[PostProcessingManager] Передан пустой Volume!");
            return;
        }

        _globalVolume = globalVolume;

        // Принудительно берем ИМЕННО .profile. Это создает экземпляр-клон в рантайме.
        VolumeProfile runtimeProfile = _globalVolume.profile;
    }

    public void SetGamma(float gammaValue)
    {
        if (_liftGammaGain != null)
        {
            _liftGammaGain.gamma.overrideState = true;

            Vector4 currentGamma = _liftGammaGain.gamma.value;
            currentGamma.w = gammaValue;
            _liftGammaGain.gamma.value = currentGamma;
        }
    }

    public void SwitchMotionBlur(bool enable)
    {
        if (_motionBlur != null)
        {
            _motionBlur.active = enable;
            _motionBlur.intensity.overrideState = true;
            _motionBlur.intensity.value = enable ? 0.5f : 0f;
        }
    }
}
