using UnityEngine;
using UnityTutorial.PlayerControl;
using Zenject;

/// <summary>
/// ��������� ���������� ����
/// </summary>
public class GameScenario : IInitializable, System.IDisposable
{
    // �����������
    private readonly EventManager _eventManager;
    private readonly PlayerController _playerController;

    public static GameScenario Instance { get; private set; }

    public GameScenario(EventManager eventBus, PlayerController playerController)
    {
        _eventManager = eventBus;
        _playerController = playerController;
    }

    public void Initialize()
    {
        _eventManager.OnMiniGameCompleted += OnMiniGameCompleted;
    }

    public void Dispose()
    {
        _eventManager.OnMiniGameCompleted -= OnMiniGameCompleted;
    }

    private void OnMiniGameCompleted()
    {
        ResumeMainGame();
    }

    public void PauseMainGame()
    {
        // ��������� ����������
        _playerController.gameObject.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeMainGame(bool fadeFromBlack = true, bool enablePlayerController = true)
    {
        if (enablePlayerController)
        {
            // �������� ����������
            _playerController.gameObject.SetActive(true);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (fadeFromBlack)
        {
            // ��������� ������� ��������� �������� �����
            ScreenFaderTransition.Instance.FadeFromBlack();
        }
    }
}
