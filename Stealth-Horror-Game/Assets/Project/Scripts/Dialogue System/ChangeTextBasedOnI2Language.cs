using I2.Loc;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextBasedOnI2Language : MonoBehaviour
{
    Text subtitleText;
    TextMeshProUGUI subtitleTextMeshPro;

    [Header("Standard Fonts")]
    [SerializeField] Font englishFont;
    [SerializeField] Font russianFont;
    [SerializeField] Font chineseFont;

    [Header("TMP Fonts")]
    [SerializeField] TMP_FontAsset englishTMPFont;
    [SerializeField] TMP_FontAsset russianTMPFont;
    [SerializeField] TMP_FontAsset chineseTMPFont;

    private string lastAppliedLanguage;

    private void Awake()
    {
        subtitleText = GetComponent<Text>();
        subtitleTextMeshPro = GetComponent<TextMeshProUGUI>();
        lastAppliedLanguage = LocalizationManager.CurrentLanguage;
        UpdateFontForCurrentLanguage();
    }

    private void OnEnable()
    {
        UpdateFontForCurrentLanguage();
        LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationStarted += OnConversationStarted;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
    }

    private void Update()
    {
        //  аждый кадр провер€ем, не устарел ли текст в открытых панел€х.
        // Ёто единственный надЄжный способ, т.к. Dialogue System перезаписывает
        // Portrait Name из своего кэша при каждой новой реплике.
        if (DialogueManager.isConversationActive)
            SyncAllPanelNames();
    }

    private void OnLanguageChanged()
    {
        lastAppliedLanguage = LocalizationManager.CurrentLanguage;
        UpdateFontForCurrentLanguage();

        if (DialogueManager.isConversationActive)
        {
            DialogueManager.SetLanguage(lastAppliedLanguage);
            // Ќе вызываем здесь ничего больше Ч Update() сам подхватит
            // изменени€ на следующем кадре.
        }
    }

    private void OnConversationStarted(Transform actor)
    {
        // язык мог помен€тьс€ до старта разговора Ч примен€ем сразу
        if (LocalizationManager.CurrentLanguage != lastAppliedLanguage)
            OnLanguageChanged();

        if (DialogueManager.isConversationActive)
            DialogueManager.SetLanguage(LocalizationManager.CurrentLanguage);
    }

    /// <summary>
    /// —инхронизирует имена во всех открытых панел€х субтитров с текущим €зыком.
    /// </summary>
    private void SyncAllPanelNames()
    {
        if (DialogueManager.standardDialogueUI == null) return;

        try
        {
            var panels = DialogueManager.standardDialogueUI.conversationUIElements.subtitlePanels;
            for (int i = 0; i < panels.Length; i++)
            {
                var panel = panels[i];
                if (panel == null || panel.portraitName == null || !panel.isOpen) continue;

                string correct = GetLocalizedNameForPanel(panel);
                if (!string.IsNullOrEmpty(correct) && panel.portraitName.text != correct)
                {
                    panel.portraitName.text = correct;
                }

                //  нопка Continue Ч единственна€ причина, зачем вы еЄ трогали.
                // ≈сли она не нужна, эту строку можно убрать.
                if (panel.continueButton != null && !panel.continueButton.gameObject.activeSelf)
                    panel.continueButton.gameObject.SetActive(true);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ќшибка синхронизации имЄн панелей: {e.Message}");
        }
    }

    private string GetLocalizedNameForPanel(StandardUISubtitlePanel panel)
    {
        // ѕробуем получить им€ через активный субтитр
        if (panel.currentSubtitle != null && panel.currentSubtitle.speakerInfo != null)
        {
            return PixelCrushers.DialogueSystem.CharacterInfo
                .GetLocalizedDisplayNameInDatabase(panel.currentSubtitle.speakerInfo.nameInDatabase);
        }

        // Fallback Ч через участников разговора
        if (!DialogueManager.isConversationActive) return null;

        var model = DialogueManager.conversationModel;
        if (model?.actorInfo != null)
        {
            return PixelCrushers.DialogueSystem.CharacterInfo
                .GetLocalizedDisplayNameInDatabase(model.actorInfo.nameInDatabase);
        }
        if (model?.conversantInfo != null)
        {
            return PixelCrushers.DialogueSystem.CharacterInfo
                .GetLocalizedDisplayNameInDatabase(model.conversantInfo.nameInDatabase);
        }

        return null;
    }

    private void UpdateFontForCurrentLanguage()
    {
        Font targetFont = GetFontForLanguage(LocalizationManager.CurrentLanguage);
        TMP_FontAsset tmpFont = GetTMPFontForLanguage(LocalizationManager.CurrentLanguage);

        if (subtitleText != null && targetFont != null)
            subtitleText.font = targetFont;
        else if (subtitleTextMeshPro != null && tmpFont != null)
            subtitleTextMeshPro.font = tmpFont;
    }

    private Font GetFontForLanguage(string language)
    {
        if (string.IsNullOrEmpty(language)) return englishFont;
        string l = language.ToLower();
        if (l.Contains("russian") || l.Contains("ru")) return russianFont;
        if (l.Contains("chinese") || l.Contains("zh")) return chineseFont;
        return englishFont;
    }

    private TMP_FontAsset GetTMPFontForLanguage(string language)
    {
        if (string.IsNullOrEmpty(language)) return englishTMPFont;
        string l = language.ToLower();
        if (l.Contains("russian") || l.Contains("ru")) return russianTMPFont;
        if (l.Contains("chinese") || l.Contains("zh")) return chineseTMPFont;
        return englishTMPFont;
    }

    public void ForceLanguageUpdate() => OnLanguageChanged();
}