using PixelCrushers.DialogueSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityTutorial.PlayerControl;
using Zenject;

/// <summary>
/// Компонент для любого предмета, который можно подобрать.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour, IInteractable
{
    private Outline _outline;
    private AudioSource _audioSource;
    private GameScenario _gameScenario;
    private PlayerController _playerController;
    private EventManager _eventManager;

    [SerializeField] private string commentText;
    [SerializeField] private string miniGameScene;
    [SerializeField] private string conversation;
    [SerializeField] private CutsceneData cutscene;

    private bool _canBeInteracted = true;

    [Inject]
    public void Construct(GameScenario gameScenario, PlayerController playerController, EventManager eventManager)
    {
        _gameScenario = gameScenario;
        _playerController = playerController;
        _eventManager = eventManager;
    }

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (_outline != null)
            _outline.enabled = false;
    }

    public void OnFocus()
    {
        InteractionPromptManager.Instance.ShowHint(transform);
        if (_outline != null)
            _outline.enabled = true;
    }

    public void OnLoseFocus()
    {
        InteractionPromptManager.Instance.HideHint();
        if (_outline != null)
            _outline.enabled = false;
    }

    public bool CanBeInteracted()
    {
        return _canBeInteracted;
    }

    public void SetCanBeInteracted(bool value)
    {
        _canBeInteracted = value;
    }

    private IEnumerator SwitchCanBeInteracted(float switchDelay)
    {
        SetCanBeInteracted(false);
        yield return new WaitForSeconds(switchDelay);
        SetCanBeInteracted(true);
    }

    private void ResetFlags()
    {
        StopAllCoroutines();
        SetCanBeInteracted(false);
    }

    public void Interact()
    {
        if (!_canBeInteracted)
        {
            Debug.LogWarning("CanBeInteracted disabled!");
            return;
        }

        if (_audioSource != null)
            _audioSource.Play();

        OnLoseFocus();

        if (cutscene != null)
        {
            CutsceneManager.Instance.PlayNewCutscene(cutscene);
        }
        else if (!string.IsNullOrEmpty(conversation))
        {
            DialogueManager.StartConversation(conversation, _playerController.transform, transform);
            _eventManager.ReportTalkingStarted();
        }
        else if (!string.IsNullOrEmpty(miniGameScene))
        {
            StartCoroutine(SwitchCanBeInteracted(5));
            // Запускаем переход
            ScreenFaderTransition.Instance.FadeToBlack(miniGameScene, LoadSceneMode.Additive, () =>
            {
                _gameScenario.PauseMainGame();
            });
        }
        else
        {
            InteractionPromptManager.Instance.ShowComment(commentText);
        }
    }

    public void OnInteractionEnded()
    {
        if (!string.IsNullOrEmpty(conversation))
        {
            _eventManager.ReportTalkingEnded();
        }
    }
}