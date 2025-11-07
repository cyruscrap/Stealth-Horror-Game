using I2.Loc;
using PixelCrushers.DialogueSystem;
using System.Collections;
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

    private bool isChangingLanguage = false;
    private string currentLanguage;

    private void Awake()
    {
        subtitleText = GetComponent<Text>();
        subtitleTextMeshPro = GetComponent<TextMeshProUGUI>();
        currentLanguage = LocalizationManager.CurrentLanguage;

        // Устанавливаем начальный язык
        ApplyLanguageImmediately();
    }

    private void OnEnable()
    {
        UpdateFontForCurrentLanguage();
        LocalizationManager.OnLocalizeEvent += UpdateFontForCurrentLanguage;
        LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
        DialogueManager.instance.conversationStarted += OnConversationStarted;
    }

    private void OnDisable()
    {
        UpdateFontForCurrentLanguage();
        LocalizationManager.OnLocalizeEvent -= UpdateFontForCurrentLanguage;
        LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
        }

    }

    private void Update()
    {
        // Простое непрерывное обновление панелей
        if (DialogueManager.isConversationActive)
        {
            UpdatePanelNamesContinuous();
        }
    }

    private void OnConversationStarted(Transform actor)
    {
        Debug.Log("Разговор начат - применяем текущий язык");
        // Гарантируем, что язык установлен правильно при начале разговора
        ApplyLanguageImmediately();
    }

    private void OnLanguageChanged()
    {
        if (isChangingLanguage) return;

        string newLanguage = LocalizationManager.CurrentLanguage;
        if (newLanguage == currentLanguage) return;

        currentLanguage = newLanguage;
        StartCoroutine(ApplyLanguageChangeCoroutine());
    }

    private IEnumerator ApplyLanguageChangeCoroutine()
    {
        isChangingLanguage = true;

        Debug.Log($"Язык изменен на: {currentLanguage}");

        yield return new WaitForEndOfFrame();

        // Применяем язык немедленно
        ApplyLanguageImmediately();

        Debug.Log("Обновление языка применено");
        isChangingLanguage = false;
    }

    /// <summary>
    /// Немедленно применяет текущий язык ко всем системам
    /// </summary>
    private void ApplyLanguageImmediately()
    {
        // 1. Устанавливаем язык в Dialogue System
        DialogueManager.SetLanguage(currentLanguage);

        // 2. Обновляем шрифты
        UpdateFontForCurrentLanguage();

        // 3. Если разговор активен - обновляем всё
        if (DialogueManager.isConversationActive)
        {
            UpdateCharacterNames();
            UpdateAllPanelNames();
            RefreshActiveConversation();
        }

        Debug.Log($"Язык применен: {currentLanguage}");
    }

    /// <summary>
    /// Непрерывно обновляет имена в панелях
    /// </summary>
    private void UpdatePanelNamesContinuous()
    {
        if (DialogueManager.standardDialogueUI == null) return;

        try
        {
            var panels = DialogueManager.standardDialogueUI.conversationUIElements.subtitlePanels;
            foreach (var panel in panels)
            {
                if (panel != null && panel.portraitName != null && panel.isOpen)
                {
                    UpdatePanelName(panel);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при обновлении панелей: {e.Message}");
        }
    }

    /// <summary>
    /// Обновляет имя в конкретной панели
    /// </summary>
    private void UpdatePanelName(StandardUISubtitlePanel panel)
    {
        if (panel.portraitName == null) return;

        string correctLocalizedName = GetCorrectLocalizedNameForPanel(panel);
        if (!string.IsNullOrEmpty(correctLocalizedName) && panel.portraitName.text != correctLocalizedName)
        {
            panel.portraitName.text = correctLocalizedName;
            Debug.Log($"Обновлено имя панели: {correctLocalizedName}");
        }

        Tools.SetGameObjectActive(panel.continueButton, true);
    }

    /// <summary>
    /// Принудительно обновляет все панели
    /// </summary>
    private void UpdateAllPanelNames()
    {
        if (DialogueManager.standardDialogueUI == null) return;

        try
        {
            var panels = DialogueManager.standardDialogueUI.conversationUIElements.subtitlePanels;
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    UpdatePanelName(panel);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при обновлении всех панелей: {e.Message}");
        }
    }

    /// <summary>
    /// Получает правильное локализованное имя для панели
    /// </summary>
    private string GetCorrectLocalizedNameForPanel(StandardUISubtitlePanel panel)
    {
        // Способ 1: через текущий субтитр
        if (panel.isOpen && panel.currentSubtitle != null && panel.currentSubtitle.speakerInfo != null)
        {
            var speakerInfo = panel.currentSubtitle.speakerInfo;
            return PixelCrushers.DialogueSystem.CharacterInfo.GetLocalizedDisplayNameInDatabase(speakerInfo.nameInDatabase);
        }

        // Способ 2: через участников разговора
        return GetFallbackActorName();
    }

    /// <summary>
    /// Получает имя актера через участников разговора
    /// </summary>
    private string GetFallbackActorName()
    {
        if (!DialogueManager.isConversationActive) return null;

        try
        {
            var actorInfo = DialogueManager.conversationModel?.actorInfo;
            var conversantInfo = DialogueManager.conversationModel?.conversantInfo;

            if (actorInfo != null)
            {
                return PixelCrushers.DialogueSystem.CharacterInfo.GetLocalizedDisplayNameInDatabase(actorInfo.nameInDatabase);
            }
            else if (conversantInfo != null)
            {
                return PixelCrushers.DialogueSystem.CharacterInfo.GetLocalizedDisplayNameInDatabase(conversantInfo.nameInDatabase);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при получении fallback имени: {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// Принудительно обновляет активный разговор
    /// </summary>
    private void RefreshActiveConversation()
    {
        if (!DialogueManager.isConversationActive) return;

        try
        {
            var currentState = DialogueManager.currentConversationState;
            if (currentState == null || currentState.subtitle == null) return;

            DialogueManager.standardDialogueUI.ShowSubtitle(currentState.subtitle);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при обновлении разговора: {e.Message}");
        }
    }

    public void UpdateCharacterNames()
    {
        if (!DialogueManager.isConversationActive) return;

        try
        {
            var currentState = DialogueManager.currentConversationState;
            if (currentState == null) return;

            var conversation = DialogueManager.masterDatabase.GetConversation(currentState.subtitle.dialogueEntry.conversationID);
            var actorIDs = new HashSet<int>();
            conversation.dialogueEntries.ForEach(entry => actorIDs.Add(entry.ActorID));

            foreach (var actorID in actorIDs)
            {
                var characterInfo = DialogueManager.conversationModel.GetCharacterInfo(actorID);
                string localizedName = PixelCrushers.DialogueSystem.CharacterInfo.GetLocalizedDisplayNameInDatabase(characterInfo.nameInDatabase);

                if (actorID == DialogueManager.conversationModel.actorInfo.id)
                {
                    DialogueLua.SetVariable("Player", localizedName);
                }
                else if (actorID == DialogueManager.conversationModel.conversantInfo.id)
                {
                    DialogueLua.SetVariable("Luda", localizedName);
                }
            }

            if (DialogueManager.conversationModel.actorInfo != null)
            {
                DialogueManager.conversationModel.actorInfo.Name = PixelCrushers.DialogueSystem.CharacterInfo.GetLocalizedDisplayNameInDatabase(DialogueManager.conversationModel.actorInfo.nameInDatabase);
            }

            if (DialogueManager.conversationModel.conversantInfo != null)
            {
                DialogueManager.conversationModel.conversantInfo.Name = PixelCrushers.DialogueSystem.CharacterInfo.GetLocalizedDisplayNameInDatabase(DialogueManager.conversationModel.conversantInfo.nameInDatabase);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Ошибка при обновлении имен персонажей: {e.Message}");
        }
    }

    private void UpdateFontForCurrentLanguage()
    {
        Font targetFont = GetFontForLanguage(LocalizationManager.CurrentLanguage);
        TMP_FontAsset TMP_FontAsset = GetTMPFontForLanguage(LocalizationManager.CurrentLanguage);

        if (subtitleText != null && targetFont != null)
        {
            subtitleText.font = targetFont;
        }
        else if (subtitleTextMeshPro != null && TMP_FontAsset != null)
        {
            subtitleTextMeshPro.font = TMP_FontAsset;
        }
    }

    private Font GetFontForLanguage(string language)
    {
        if (string.IsNullOrEmpty(language)) return englishFont;
        string langLower = language.ToLower();

        if (langLower.Contains("english") || langLower.Contains("en")) return englishFont;
        if (langLower.Contains("russian") || langLower.Contains("ru")) return russianFont;
        if (langLower.Contains("chinese") || langLower.Contains("zh")) return chineseFont;
        return englishFont;
    }

    private TMP_FontAsset GetTMPFontForLanguage(string language)
    {
        if (string.IsNullOrEmpty(language)) return englishTMPFont;
        string langLower = language.ToLower();

        if (langLower.Contains("english") || langLower.Contains("en")) return englishTMPFont;
        if (langLower.Contains("russian") || langLower.Contains("ru")) return russianTMPFont;
        if (langLower.Contains("chinese") || langLower.Contains("zh")) return chineseTMPFont;
        return englishTMPFont;
    }

    public void ForceLanguageUpdate()
    {
        OnLanguageChanged();
    }
}