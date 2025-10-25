using UnityEngine;
using UnityTutorial.Manager;

public class PlayerInteractionManager : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    private InputManager inputManager;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;

    [Tooltip("Радиус луча для обнаружения. Увеличивает область захвата.")]
    [SerializeField] private float interactionRadius = 0.5f;

    private IInteractable _focusedInteractable;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
        // Создаем луч из центра камеры, как и раньше
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        
        IInteractable newInteractable = null;

        // ЗАМЕНА: Используем SphereCast вместо Raycast
        if (Physics.SphereCast(ray, interactionRadius, out RaycastHit hit, interactionDistance))
        {
            hit.collider.TryGetComponent(out newInteractable);
        }

        // ОБНОВЛЕНИЕ: Правильно обрабатываем потерю фокуса
        if (newInteractable == null)
        {
            // Если ранее был объект в фокусе, но сейчас его нет - сбрасываем фокус
            if (_focusedInteractable != null)
            {
                _focusedInteractable.OnLoseFocus();
                _focusedInteractable = null;
            }
            return;
        }

        // ОБНОВЛЕНИЕ: Проверяем CanBeInteracted перед установкой фокуса
        if (newInteractable.CanBeInteracted())
        {
            // Логика управления фокусом
            if (newInteractable != _focusedInteractable)
            {
                _focusedInteractable?.OnLoseFocus(); // Сообщаем старому объекту, что он потерял фокус
                _focusedInteractable = newInteractable;
                _focusedInteractable?.OnFocus(); // Сообщаем новому, что он получил фокус
            }

            // Логика взаимодействия
            if (_focusedInteractable != null && inputManager.Interact)
            {
                _focusedInteractable.Interact();
            }
        }
        else
        {
            // ОБНОВЛЕНИЕ: Если объект найден, но с ним нельзя взаимодействовать - сбрасываем фокус
            if (_focusedInteractable != null)
            {
                _focusedInteractable.OnLoseFocus();
                _focusedInteractable = null;
            }
        }
    }

    // ОБНОВЛЕНИЕ: Дополнительная проверка при отключении компонента
    private void OnDisable()
    {
        if (_focusedInteractable != null)
        {
            _focusedInteractable.OnLoseFocus();
            _focusedInteractable = null;
        }
    }
}