using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContinueFastForward : MonoBehaviour, IPointerClickHandler
{
    public KeyCode[] continueKeys = new KeyCode[] { KeyCode.Space, KeyCode.Return };

    void Update()
    {
        foreach (var key in continueKeys)
        {
            if (InputDeviceManager.IsKeyDown(key))
            {
                FastForward();
                return;
            }
        }
    }

    // Обработчик кликов мыши на UI элемент
    public void OnPointerClick(PointerEventData eventData)
    {
        FastForward();
    }

    void FastForward()
    {
        var typewriterEffect = GetComponent<UnityUITypewriterEffect>();
        if ((typewriterEffect != null) && typewriterEffect.isPlaying)
        {
            typewriterEffect.Stop();
        }
        else
        {
            GetComponentInParent<StandardUISubtitlePanel>().OnContinue();
        }
    }
}