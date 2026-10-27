using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMP_RandomLetterBlink : MonoBehaviour
{
    private TMP_Text textComponent;
    private string lastText;
    private bool needReinit = true;

    [Header("Настройки мерцания")]
    public float minSpeed = 3f;
    public float maxSpeed = 12f;

    [Header("Диапазон прозрачности")]
    [Range(0f, 1f)] public float minAlpha = 0.1f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    private float[] letterSpeeds;
    private float[] letterOffsets;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        lastText = textComponent.text;
        InitializeRandomValues();
    }

    void InitializeRandomValues()
    {
        // Пересоздаём меш только при необходимости
        textComponent.ForceMeshUpdate();
        int characterCount = textComponent.textInfo.characterCount;

        letterSpeeds = new float[characterCount + 100];
        letterOffsets = new float[characterCount + 100];

        for (int i = 0; i < letterSpeeds.Length; i++)
        {
            letterSpeeds[i] = Random.Range(minSpeed, maxSpeed);
            letterOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
        }
        needReinit = false;
    }

    void Update()
    {
        // Проверяем, изменился ли текст
        if (textComponent.text != lastText)
        {
            lastText = textComponent.text;
            needReinit = true;
        }

        if (needReinit)
            InitializeRandomValues();
    }

    void LateUpdate()
    {
        // Применяем мерцание после всех изменений геометрии (в том числе от TextProOnACircle)
        ApplyBlink();
    }

    void ApplyBlink()
    {
        // Если текст был изменён в этом кадре, меш уже обновлён в InitializeRandomValues,
        // поэтому здесь можно работать с актуальным textInfo без ForceMeshUpdate
        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            float alphaValue = Mathf.Sin(Time.time * letterSpeeds[i] + letterOffsets[i]) * 0.5f + 0.5f;
            alphaValue = Mathf.Lerp(minAlpha, maxAlpha, alphaValue);
            byte alphaByte = (byte)(alphaValue * 255);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            vertexColors[vertexIndex + 0].a = alphaByte;
            vertexColors[vertexIndex + 1].a = alphaByte;
            vertexColors[vertexIndex + 2].a = alphaByte;
            vertexColors[vertexIndex + 3].a = alphaByte;
        }

        // Применяем изменения цвета
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}