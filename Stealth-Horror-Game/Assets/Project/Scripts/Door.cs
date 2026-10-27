using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Триггер, который переключает анимацию открытия/закрытия")]
    [SerializeField] private string interactTrigger = "Interact";

    [Header("State")]
    [Tooltip("Стартовое состояние. Сцена всегда стартует с закрытыми дверями")]
    [SerializeField] private bool startsOpen = false;

    private Animator _animator;
    private bool _isOpen;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _isOpen = startsOpen;

        // Синхронизируем Animator со стартовым состоянием
        // Если дверь стартует закрытой и Animator default = Closed — ничего делать не надо.
        // Если стартует открытой — один раз дёргаем триггер, чтобы Animator перешёл в Open.
        if (_isOpen)
            _animator.SetTrigger(interactTrigger);
    }

    /// <summary>Открыть. Если уже открыта — ничего не произойдёт.</summary>
    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        _animator.SetTrigger(interactTrigger);
    }

    /// <summary>Закрыть. Если уже закрыта — ничего не произойдёт.</summary>
    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _animator.SetTrigger(interactTrigger);
    }

    /// <summary>Переключить. Всегда меняет состояние.</summary>
    public void Toggle()
    {
        _isOpen = !_isOpen;
        _animator.SetTrigger(interactTrigger);
    }
}