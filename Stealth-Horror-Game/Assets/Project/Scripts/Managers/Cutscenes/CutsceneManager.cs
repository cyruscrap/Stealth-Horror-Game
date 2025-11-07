using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using Zenject;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    private EventManager _eventManager;
    private PlayableDirector mainPlayableDirector;
    private SubtitleText[] subtitleText;
    [SerializeField] private TextMeshProUGUI subtitles;

    [Inject]
    public void Construct(EventManager eventManager)
    {
        _eventManager = eventManager;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        mainPlayableDirector = GetComponent<PlayableDirector>();

        mainPlayableDirector.stopped += OnCutsceneEnded;
    }

    public void PlayNewCutscene(CutsceneData newCutscene)
    {
        if (mainPlayableDirector.state == PlayState.Playing)
            return;

        mainPlayableDirector.playableAsset = newCutscene.timelineAsset;
        subtitleText = newCutscene.subtitleText;

        mainPlayableDirector.Play();
        StartSubtitles();
        _eventManager.ReportTalkingStarted();
    }

    private void OnCutsceneEnded(PlayableDirector playableDirector)
    {
        _eventManager.ReportTalkingEnded();
    }

    private void StartSubtitles()
    {
        StartCoroutine(SubtitleCoroutine());
    }

    IEnumerator SubtitleCoroutine()
    {
        yield return new WaitForSeconds(1f);

        subtitles.gameObject.SetActive(true);
        foreach(var voiceLine in subtitleText)
        {
            subtitles.text = voiceLine.text;

            yield return new WaitForSecondsRealtime(voiceLine.time);
        }
        subtitles.gameObject.SetActive(false);
    }
}