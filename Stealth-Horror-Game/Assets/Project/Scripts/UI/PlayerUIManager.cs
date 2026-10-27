using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTutorial.Manager;
using Zenject;

public class PlayerUIManager : MonoBehaviour
{
    private InputManager _inputManager;
    private GameScenario _gameScenario;

    private GameOverMenu _gameOverMenu;
    private MainMenu _mainMenu;

    private GameObject _mainMenuHolder;

    [SerializeField] private GameObject taskListView;

    private bool isUIActive = false;

    [Inject]
    public void Construct(InputManager inputManager, GameScenario gameScenario)
    {
        _inputManager = inputManager;
        _gameScenario = gameScenario;
    }

    private void Awake()
    {
        _gameOverMenu = GetComponentInChildren<GameOverMenu>();
        _mainMenu = GetComponentInChildren<MainMenu>();
        _mainMenuHolder = _mainMenu.transform.parent.gameObject;
    }

    private void Start()
    {
        if (PlayerPrefsExtentions.GetBool("isNewGame"))
        {
            _gameScenario.PauseMainGame();

            _mainMenu.ShowOrHideIntroduction(true);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            _mainMenuHolder.SetActive(false);
        }
    }

    private void Update()
    {
        if (_inputManager.UIPressedThisFrame)
        {
            if (isUIActive)
            {
                isUIActive = false;
                _mainMenuHolder.gameObject.SetActive(false);
                _gameScenario.ResumeMainGame();
            }
            else
            {
                isUIActive = true;
                _mainMenuHolder.gameObject.SetActive(true);
                _gameScenario.PauseMainGame();
            }
        }
    }

    public void HideTaskListView() { taskListView.gameObject.SetActive(false); }

    public void ShowGameOverMenu()
    {
        _gameOverMenu.gameObject.SetActive(true);
        _gameScenario.PauseMainGame();
    }

    public void StartGameAfterIntroduction()
    {
        StartCoroutine(DisableUIAfterFade());
    }

    private IEnumerator DisableUIAfterFade()
    {
        ScreenFaderTransition.Instance.FadeToBlack(null, LoadSceneMode.Single, () => {
            _mainMenuHolder.SetActive(false);
            _mainMenu.ShowOrHideIntroduction(false);
        });
        yield return new WaitForSecondsRealtime(2f);
        ScreenFaderTransition.Instance.FadeFromBlack();
        _gameScenario.ResumeMainGame();
    }
}