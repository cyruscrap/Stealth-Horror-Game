using System;
using UnityEngine;
using UnityTutorial.Manager;

namespace UnityTutorial.PlayerControl
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Main Settings")]
        [SerializeField] private float animBlendSpeed = 8.9f;
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform Camera;
        [SerializeField] private Vector3 CameraOffset = new Vector3(0, .1f, .06f);
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        [SerializeField] private float MouseSensitivity = 21.9f;
        [SerializeField, Range(10, 500)] private float JumpFactor = 260f;
        [SerializeField] private float Dis2Ground = 0.8f;
        [SerializeField] private LayerMask GroundCheck;
        [SerializeField] private float AirResistance = 0.8f;

        [Header("Turn Animation Settings")]
        [SerializeField] private float turnThreshold = 0.5f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        // Компоненты
        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private PlayerAnimatorManager _playerAnimatorManager;
        private PlayerInteractionManager _playerInteractionManager;

        // Состояния
        private bool _grounded = false;

        // Переменные камеры
        private float _xRotation;

        // Движение
        [SerializeField] private float _crouchSpeed = 1.5f;
        [SerializeField] private float _walkSpeed = 2f;
        [SerializeField] private float _runSpeed = 4f;

        // Для плавности камеры
        private Vector3 _smoothCameraPosition;
        private Quaternion _smoothCameraRotation;

        // Публичные свойства для доступа из других скриптов
        public float AnimBlendSpeed => animBlendSpeed;
        public float TurnThreshold => turnThreshold;
        public float TurnSmoothTime => turnSmoothTime;
        public bool IsGrounded => _grounded;
        public InputManager Input => _inputManager;

        private void Start()
        {
            InitializeComponents();
            ResetCamera();
        }

        private void InitializeComponents()
        {
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();
            _playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            _playerInteractionManager = GetComponent<PlayerInteractionManager>();
        }

        private void ResetCamera()
        {
            // Инициализируем плавные значения
            _smoothCameraPosition = CameraRoot.position + CameraRoot.TransformDirection(CameraOffset);
            _smoothCameraRotation = Quaternion.Euler(_xRotation, transform.eulerAngles.y, 0);

            Camera.position = _smoothCameraPosition;
            Camera.rotation = _smoothCameraRotation;
        }

        private void FixedUpdate()
        {
            SampleGround();
            Move();
            HandleJump();
            HandleCrouch();
            _playerAnimatorManager.HandleTurnAnimation();
        }

        private void Update()
        {
            HandleCamera();
            _playerAnimatorManager.UpdateAnimationParameters();
        }

        private void Move()
        {
            float targetSpeed = GetTargetSpeed();

            Vector3 cameraForward = Camera.forward;
            Vector3 cameraRight = Camera.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * _inputManager.Move.y + cameraRight * _inputManager.Move.x).normalized;

            if (_grounded)
            {
                Vector3 targetVelocity = moveDirection * targetSpeed;
                Vector3 horizontalVelocity = new Vector3(_playerRigidbody.linearVelocity.x, 0, _playerRigidbody.linearVelocity.z);
                Vector3 velocityDifference = targetVelocity - horizontalVelocity;

                float acceleration = _inputManager.Run ? 20f : 15f;
                _playerRigidbody.AddForce(velocityDifference * acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
            else
            {
                if (_inputManager.Move != Vector2.zero)
                {
                    Vector3 targetVelocity = moveDirection * targetSpeed * 0.3f;
                    Vector3 velocityDifference = targetVelocity - new Vector3(_playerRigidbody.linearVelocity.x, 0, _playerRigidbody.linearVelocity.z);
                    _playerRigidbody.AddForce(velocityDifference * AirResistance * 5f * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
        }

        private float GetTargetSpeed()
        {
            if (_inputManager.Move == Vector2.zero) return 0f;
            if (_inputManager.Crouch) return _crouchSpeed;
            return _inputManager.Run ? _runSpeed : _walkSpeed;
        }

        private void HandleCamera()
        {
            var mouseX = _inputManager.Look.x;
            var mouseY = _inputManager.Look.y;

            float sensitivityMultiplier = MouseSensitivity * 0.1f;
            float mouseXDelta = mouseX * sensitivityMultiplier;
            float mouseYDelta = mouseY * sensitivityMultiplier;

            // Вращение камеры
            _xRotation -= mouseYDelta;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);
            transform.Rotate(0, mouseXDelta, 0);

            // Базовая позиция камеры (без тряски)
            Vector3 targetPosition = CameraRoot.position + CameraRoot.TransformDirection(CameraOffset);
            Quaternion targetRotation = Quaternion.Euler(_xRotation, transform.eulerAngles.y, 0);

            // Плавное следование камеры за целевой позицией
            _smoothCameraPosition = Vector3.Lerp(_smoothCameraPosition, targetPosition, 15f * Time.deltaTime);
            _smoothCameraRotation = Quaternion.Slerp(_smoothCameraRotation, targetRotation, 12f * Time.deltaTime);

            // Устанавливаем позицию и вращение камеры
            Camera.position = _smoothCameraPosition;
            Camera.rotation = _smoothCameraRotation;
        }

        private void HandleCrouch()
        {
            _playerAnimatorManager.HandleCrouch(_inputManager.Crouch);
        }

        private void HandleJump()
        {
            if (!_inputManager.Jump || !_grounded) return;
            _playerAnimatorManager.HandleJump();
        }

        public void JumpAddForce()
        {
            _playerRigidbody.AddForce(-_playerRigidbody.linearVelocity.y * Vector3.up, ForceMode.VelocityChange);
            _playerRigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
            _playerAnimatorManager.ResetJumpTrigger();
        }

        private void SampleGround()
        {
            bool wasGrounded = _grounded;
            _grounded = Physics.Raycast(_playerRigidbody.worldCenterOfMass, Vector3.down, Dis2Ground + 0.1f, GroundCheck);

            if (wasGrounded != _grounded)
            {
                _playerAnimatorManager.SetAnimationGrounding(_grounded);
            }

            if (!_grounded)
            {
                _playerAnimatorManager.SetFallingVelocity(_playerRigidbody.linearVelocity.y);
            }
        }
    }
}