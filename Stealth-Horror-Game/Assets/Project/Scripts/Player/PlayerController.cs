using UnityEngine;
using UnityTutorial.Manager;
using Zenject;

namespace UnityTutorial.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        private EventManager _eventManager;

        [Header("Main Settings")]
        [SerializeField] private float animBlendSpeed = 8.9f;
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform CameraRootNoShake;
        [SerializeField] private Transform Camera;
        [SerializeField] private Vector3 CameraOffset = new Vector3(0, .1f, .3f);
        [SerializeField] private Vector3 runCameraOffset = new Vector3(0, 0, 0.15f);
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        [SerializeField] private float mouseSensitivity = 0.5f;
        [SerializeField] private bool invertXAxis = false;
        [SerializeField] private bool invertYAxis = false;
        [SerializeField] private bool cameraShake = true;

        [Header("Turn Animation Settings")]
        [SerializeField] private float turnThreshold = 0.5f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        [Header("Gravity & Jump")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;

        [Header("Movement Speeds (meters per second)")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 4f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float globalSpeedMultiplier = 1f; // дополнительный множитель для всех скоростей

        [Header("Root Motion Tuning")]
        [SerializeField] private float rootMotionSmoothing = 0.1f;   // плавность старта/остановки

        [Header("Manual Movement Compensation (backward/side)")]
        [SerializeField] private float manualMoveSpeed = 0.3f;      // базовая скорость компенсации
        [SerializeField] private float manualMoveFactor = 0.2f;     // доля от базовой

        // Компоненты
        private CharacterController _controller;
        private Animator _animator;
        private InputManager _inputManager;
        private PlayerAnimatorManager _playerAnimatorManager;
        private PlayerInteractionManager _playerInteractionManager;

        // Состояния
        private bool _grounded = false;
        private bool _canMove = true;
        private bool _cutsceneMode;
        private bool _cameraInitialized;

        // Камера
        private float _xRotation;
        private Vector3 _smoothCameraPosition;
        private Quaternion _smoothCameraRotation;

        // Гравитация
        private float _velocityY;

        // Сглаженное перемещение от Root Motion (горизонтальное)
        private Vector3 _smoothedRootMotion;

        //Контроллер
        private float baseControllerCenter = .9f;
        private float baseControllerHeight = 1.64f;
        private float crouchControllerCenter = .6f;
        private float crouchControllerHeight = 1.14f;

        // Кэшированные направления камеры (для оптимизации)
        private Vector3 _cachedCameraForward;
        private Vector3 _cachedCameraRight;

        // Публичные свойства
        public float AnimBlendSpeed => animBlendSpeed;
        public float TurnThreshold => turnThreshold;
        public float TurnSmoothTime => turnSmoothTime;
        public bool IsGrounded => _grounded;
        public bool CanMove => _canMove;
        public InputManager Input => _inputManager;
        public bool InvertXAxis
        {
            get { return invertXAxis; }
            set { invertXAxis = value; }
        }
        public bool InvertYAxis
        {
            get { return invertYAxis; }
            set { invertYAxis = value; }
        }
        public bool CameraShake
        {
            get { return cameraShake; }
            set { cameraShake = value; }
        }
        public float MouseSensitivity
        {
            get { return mouseSensitivity; }
            set
            {
                mouseSensitivity = Mathf.Clamp(value, 0, 1);
            }
        }

        [Inject]
        public void Construct(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        private void OnEnable()
        {
            _eventManager.OnTalkingStarted += StopMove;
            _eventManager.OnTalkingEnded += StartMove;
        }

        private void OnDisable()
        {
            _eventManager.OnTalkingStarted -= StopMove;
            _eventManager.OnTalkingEnded -= StartMove;
        }

        private void Start()
        {
            InitializeComponents();
            StartMove();
        }

        private void InitializeComponents()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _inputManager = GetComponent<InputManager>();
            _playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            _playerInteractionManager = GetComponent<PlayerInteractionManager>();
        }

        private void StopMove()
        {
            _playerAnimatorManager.StopMovement();
            _velocityY = 0f;
            _smoothedRootMotion = Vector3.zero;
            _canMove = false;
        }

        private void StartMove()
        {
            _canMove = true;
        }

        private void ResetCamera()
        {
            if (cameraShake) _smoothCameraPosition = CameraRoot.position + CameraRoot.TransformDirection(CameraOffset);
            else _smoothCameraPosition = CameraRootNoShake.position + CameraRootNoShake.TransformDirection(CameraOffset);
            _smoothCameraRotation = Quaternion.Euler(_xRotation, transform.eulerAngles.y, 0);
            Camera.position = _smoothCameraPosition;
            Camera.rotation = _smoothCameraRotation;
        }

        public void EnterCutsceneMode()
        {
            _cutsceneMode = true;

            // Сбрасываем накопленные скорости
            _smoothedRootMotion = Vector3.zero;
            _velocityY = 0f;

            // Отключаем физический контроллер — он не даст Timeline двигать объект
            if (_controller != null) _controller.enabled = false;

            // Отключаем root motion — иначе Animator будет двигать сам
            if (_animator != null) _animator.applyRootMotion = false;
        }

        public void ExitCutsceneMode()
        {
            _cutsceneMode = false;

            if (_controller != null) _controller.enabled = true;
            if (_animator != null) _animator.applyRootMotion = true;
        }

        private void Update()
        {
            if (_cutsceneMode) return;
            if (!_canMove) return;

            // Кэшируем направления камеры для FixedUpdate
            CacheCameraDirections();

            // Горизонтальный поворот от мыши
            HandleCameraRotation();

            // Приседание
            HandleCrouch();

            // Обновление параметров анимации (скорости, повороты и т.д.)
            _playerAnimatorManager.UpdateAnimationParameters();
            _playerAnimatorManager.HandleTurnAnimation();
        }

        private void FixedUpdate()
        {
            if (_cutsceneMode) return;
            if (!_canMove) return;

            // 1. Получаем направление от Root Motion и масштабируем до целевой скорости
            Vector3 rawRootMotion = _animator.deltaPosition;
            rawRootMotion.y = 0;

            float targetSpeed = GetTargetSpeed() * globalSpeedMultiplier;

            if (rawRootMotion.sqrMagnitude > 0.0001f && targetSpeed > 0)
            {
                Vector3 direction = rawRootMotion.normalized;
                Vector3 targetMove = direction * targetSpeed * Time.fixedDeltaTime;

                // Сглаживаем изменение (плавный старт/остановка)
                float smoothFactor = 1f - Mathf.Exp(-rootMotionSmoothing * 20f * Time.fixedDeltaTime);
                _smoothedRootMotion = Vector3.Lerp(_smoothedRootMotion, targetMove, smoothFactor);
            }
            else
            {
                // Быстрое затухание при остановке
                _smoothedRootMotion = Vector3.MoveTowards(_smoothedRootMotion, Vector3.zero, 15f * Time.fixedDeltaTime);
                if (_smoothedRootMotion.sqrMagnitude < 0.000001f)
                    _smoothedRootMotion = Vector3.zero;
            }

            // 2. Ручная компенсация для движения назад/вбок (если анимация не даёт перемещения)
            Vector3 manualMove = Vector3.zero;
            if (_inputManager.Move.sqrMagnitude > 0f && _grounded && targetSpeed > 0)
            {
                Vector3 moveDirection = (_cachedCameraForward * _inputManager.Move.y + _cachedCameraRight * _inputManager.Move.x);
                moveDirection.Normalize();

                float forwardDot = Vector3.Dot(moveDirection, transform.forward);
                // Если направление не вперёд (назад или вбок), добавляем небольшую компенсацию
                if (forwardDot < 0.3f)
                {
                    manualMove = moveDirection * manualMoveSpeed * manualMoveFactor * Time.fixedDeltaTime;
                }
            }

            // 3. Гравитация и прыжок
            HandleGravityAndJump();

            // 4. Итоговое перемещение
            Vector3 verticalMove = new Vector3(0, _velocityY, 0) * Time.fixedDeltaTime;
            Vector3 totalMove = _smoothedRootMotion + manualMove + verticalMove;
            _controller.Move(totalMove);

            // 5. Обновляем grounded
            bool wasGrounded = _grounded;
            _grounded = _controller.isGrounded;
            if (wasGrounded != _grounded)
            {
                _playerAnimatorManager.SetAnimationGrounding(_grounded);
            }

            if (!_grounded)
            {
                _playerAnimatorManager.SetFallingVelocity(_velocityY);
            }
        }

        private void LateUpdate()
        {
            if (_cutsceneMode) return;
            if (!_canMove) return;

            Vector3 basePosition = cameraShake
                ? CameraRoot.position + CameraRoot.TransformDirection(CameraOffset)
                : CameraRootNoShake.position + CameraRootNoShake.TransformDirection(CameraOffset);

            if (_inputManager.Run && _grounded)
                basePosition += transform.forward * runCameraOffset.z;

            Vector3 targetPosition = basePosition;
            Quaternion targetRotation = Quaternion.Euler(_xRotation, transform.eulerAngles.y, 0);

            if (!_cameraInitialized)
            {
                // Жёсткий снап на первом же кадре, когда позиция игрока уже финальная
                _cameraInitialized = true;
                _smoothCameraPosition = targetPosition;
                _smoothCameraRotation = targetRotation;
            }
            else
            {
                _smoothCameraPosition = Vector3.Lerp(_smoothCameraPosition, targetPosition, 15f * Time.deltaTime);
                _smoothCameraRotation = Quaternion.Slerp(_smoothCameraRotation, targetRotation, 12f * Time.deltaTime);
            }

            Camera.position = _smoothCameraPosition;
            Camera.rotation = _smoothCameraRotation;
        }

        private void CacheCameraDirections()
        {
            _cachedCameraForward = Camera.forward;
            _cachedCameraRight = Camera.right;
            _cachedCameraForward.y = 0f;
            _cachedCameraRight.y = 0f;
            _cachedCameraForward.Normalize();
            _cachedCameraRight.Normalize();
        }

        private void HandleCameraRotation()
        {
            float mouseX = invertXAxis ? -_inputManager.Look.x : _inputManager.Look.x;
            float mouseY = invertYAxis ? -_inputManager.Look.y : _inputManager.Look.y;
            float sensitivityMultiplier = mouseSensitivity * 0.1f;
            _xRotation -= mouseY * sensitivityMultiplier;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);
            transform.Rotate(0, mouseX * sensitivityMultiplier, 0);
        }

        private float GetTargetSpeed()
        {
            if (_inputManager.Move.sqrMagnitude == 0f) return 0f;
            if (_inputManager.Crouch) return crouchSpeed;
            return _inputManager.Run ? runSpeed : walkSpeed;
        }

        private void HandleGravityAndJump()
        {
            bool isGrounded = _controller.isGrounded;

            if (isGrounded && _velocityY < 0)
            {
                _velocityY = -0.5f;
            }

            if (isGrounded && _inputManager.Jump)
            {
                _velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _playerAnimatorManager.HandleJump();
            }

            if (_velocityY < 0)
            {
                _velocityY += gravity * fallMultiplier * Time.fixedDeltaTime;
            }
            else if (_velocityY > 0 && !_inputManager.Jump)
            {
                _velocityY += gravity * lowJumpMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                _velocityY += gravity * Time.fixedDeltaTime;
            }

            _velocityY = Mathf.Max(_velocityY, -20f);
        }

        private void HandleCrouch()
        {
            float targetHeight = _inputManager.Crouch ? crouchControllerHeight : baseControllerHeight;
            _controller.height = Mathf.Lerp(_controller.height, targetHeight, 8f * Time.deltaTime);

            float targetCenter = _inputManager.Crouch ? crouchControllerCenter : baseControllerCenter;
            _controller.center = Vector3.Lerp(_controller.center, new Vector3(0, targetCenter, 0), 8f * Time.deltaTime);

            _playerAnimatorManager.HandleCrouch(_inputManager.Crouch);
        }

        // Вызывается из анимации (Animation Event) для сброса триггера прыжка
        public void ResetJumpTrigger()
        {
            _playerAnimatorManager.ResetJumpTrigger();
        }
    }
}