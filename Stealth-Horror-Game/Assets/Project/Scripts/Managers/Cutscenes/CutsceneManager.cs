using I2.Loc;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityTutorial.PlayerControl;
using Zenject;

public class CutsceneManager : MonoBehaviour
{
    private PlayerController _playerController;
    private EventManager _eventManager;
    private WorldSaveGameManager _saveManager;
    private AudioSource _audioSource;
    private PlayableDirector _mainPlayableDirector;

    private string currentCutsceneID;
    private UnityEvent currentCutsceneEndEvent;
    private string currentTextKeyAfterCutscene;
    private SubtitleText[] subtitleText;

    [SerializeField] private Image panel;
    [SerializeField] private TextMeshProUGUI subtitles;
    [SerializeField] private float subtitleDelay = 1f;

    [Inject]
    public void Construct(PlayerController playerController,
                          EventManager eventManager,
                          WorldSaveGameManager saveManager)
    {
        _playerController = playerController;
        _eventManager = eventManager;
        _saveManager = saveManager;
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _mainPlayableDirector = GetComponent<PlayableDirector>();

        _mainPlayableDirector.stopped += OnCutsceneEnded;
        _mainPlayableDirector.stopped += _ => _playerController.ExitCutsceneMode();

        subtitles.text = "";
    }

    public void SwitchSubtitles(bool enable)
    {
        subtitles.gameObject.SetActive(enable);
    }

    public void PlayNewCutscene(CutsceneData newCutscene, UnityEvent onCutsceneEnded = null)
    {
        if (_mainPlayableDirector.state == PlayState.Playing)
            return;

        _playerController.EnterCutsceneMode();

        currentCutsceneID = newCutscene.cutsceneID;
        _mainPlayableDirector.playableAsset = newCutscene.timelineAsset;
        subtitleText = newCutscene.subtitleText;
        currentTextKeyAfterCutscene = newCutscene.textKeyAfterCutscene;
        currentCutsceneEndEvent = onCutsceneEnded;

        _mainPlayableDirector.Play();
        StartSubtitles();
        _eventManager.ReportTalkingStarted();
    }

    private void OnCutsceneEnded(PlayableDirector playableDirector)
    {
        ScreenFaderTransition.Instance.FadeFromBlack();
        _eventManager.ReportTalkingEnded();

        if (!string.IsNullOrEmpty(currentTextKeyAfterCutscene))
        {
            InteractionPromptManager.Instance.ShowComment(currentTextKeyAfterCutscene, subtitleDelay);
            currentTextKeyAfterCutscene = "";
        }

        if (currentCutsceneEndEvent != null)
        {
            currentCutsceneEndEvent.Invoke();
            currentCutsceneEndEvent = null;
        }

        if (!string.IsNullOrEmpty(currentCutsceneID))
        {
            CharacterSaveData data = _saveManager.currentCharacterSaveData;
            if (data.playedCutscenes.Contains(currentCutsceneID) == false)
            {
                data.playedCutscenes.Add(currentCutsceneID);
                _saveManager.NotifyStateChanged();
            }
            currentCutsceneID = null;
        }
    }

    public bool HasCutsceneBeenPlayed(string cutsceneID)
    {
        if (string.IsNullOrEmpty(cutsceneID)) return false;
        return _saveManager.currentCharacterSaveData.playedCutscenes.Contains(cutsceneID);
    }

    private void StartSubtitles() => StartCoroutine(SubtitleCoroutine());

    IEnumerator SubtitleCoroutine()
    {
        yield return new WaitForSeconds(subtitleDelay);

        foreach (SubtitleText voiceLine in subtitleText)
        {
            subtitles.text = LocalizationManager.GetTranslation(voiceLine.textKey);
            if (!string.IsNullOrEmpty(voiceLine.audioClipKey))
            {
                AudioClip audioClip = LocalizationManager.GetTranslatedObjectByTermName<AudioClip>(voiceLine.audioClipKey);
                if (audioClip != null) _audioSource.PlayOneShot(audioClip);
            }
            yield return new WaitForSecondsRealtime(voiceLine.time);
        }

        subtitles.text = "";
    }
}