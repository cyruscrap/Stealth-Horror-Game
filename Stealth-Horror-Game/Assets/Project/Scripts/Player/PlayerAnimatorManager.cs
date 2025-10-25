using UnityEngine;
using UnityTutorial.Manager;

namespace UnityTutorial.PlayerControl
{
    public class PlayerAnimatorManager : MonoBehaviour
    {
        private PlayerController _playerController;
        private InputManager _inputManager;
        private Animator _animator;

        // Хэши аниматора
        private int _xVelHash;
        private int _yVelHash;
        private int _jumpHash;
        private int _groundHash;
        private int _fallingHash;
        private int _zVelHash;
        private int _crouchHash;
        private int _turnHash;
        private int _isMovingHash;

        // Переменные для анимаций (только здесь!)
        private Vector2 _animationVelocity;
        private float _previousRotationY;
        private float _turnVelocity;
        private float _currentTurnValue;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _inputManager = GetComponent<InputManager>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            InitializeAnimatorHashes();
            _previousRotationY = transform.eulerAngles.y;
        }

        private void InitializeAnimatorHashes()
        {
            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
            _zVelHash = Animator.StringToHash("Z_Velocity");
            _jumpHash = Animator.StringToHash("Jump");
            _groundHash = Animator.StringToHash("Grounded");
            _fallingHash = Animator.StringToHash("Falling");
            _crouchHash = Animator.StringToHash("Crouch");
            _turnHash = Animator.StringToHash("Turn");
            _isMovingHash = Animator.StringToHash("IsMoving");
        }

        public void UpdateAnimationParameters()
        {
            if (_inputManager.Move != Vector2.zero)
            {
                float animationMultiplier = 0.5f; // Ходьба

                if (_inputManager.Run)
                {
                    animationMultiplier = 1f; // Бег
                }
                else if (_inputManager.Crouch)
                {
                    animationMultiplier = 0.3f; // Присед
                }

                // Используем настройку из PlayerController
                float blendSpeed = _playerController.AnimBlendSpeed * 2f;
                _animationVelocity.x = Mathf.Lerp(_animationVelocity.x, _inputManager.Move.x * animationMultiplier, blendSpeed * Time.deltaTime);
                _animationVelocity.y = Mathf.Lerp(_animationVelocity.y, _inputManager.Move.y * animationMultiplier, blendSpeed * Time.deltaTime);
            }
            else
            {
                _animationVelocity.x = Mathf.Lerp(_animationVelocity.x, 0f, _playerController.AnimBlendSpeed * Time.deltaTime);
                _animationVelocity.y = Mathf.Lerp(_animationVelocity.y, 0f, _playerController.AnimBlendSpeed * Time.deltaTime);
            }

            _animator.SetFloat(_xVelHash, _animationVelocity.x, 0.5f, Time.deltaTime);
            _animator.SetFloat(_yVelHash, _animationVelocity.y, 0.5f, Time.deltaTime);
        }

        public void HandleTurnAnimation()
        {
            if (!_playerController.IsGrounded) return;

            bool isMoving = _inputManager.Move != Vector2.zero;
            _animator.SetBool(_isMovingHash, isMoving);

            if (isMoving)
            {
                _currentTurnValue = Mathf.SmoothDamp(_currentTurnValue, 0f, ref _turnVelocity, _playerController.TurnSmoothTime);
                _animator.SetFloat(_turnHash, _currentTurnValue, 0.1f, Time.deltaTime);
                _previousRotationY = transform.eulerAngles.y;
                return;
            }

            float currentRotationY = transform.eulerAngles.y;
            float rotationDifference = Mathf.DeltaAngle(_previousRotationY, currentRotationY);
            _previousRotationY = currentRotationY;

            if (Mathf.Abs(rotationDifference) > _playerController.TurnThreshold)
            {
                float targetTurnValue = Mathf.Clamp(rotationDifference * 0.8f, -1f, 1f);
                _currentTurnValue = Mathf.SmoothDamp(_currentTurnValue, targetTurnValue, ref _turnVelocity, _playerController.TurnSmoothTime);
            }
            else
            {
                _currentTurnValue = Mathf.SmoothDamp(_currentTurnValue, 0f, ref _turnVelocity, _playerController.TurnSmoothTime * 0.5f);
            }

            _animator.SetFloat(_turnHash, _currentTurnValue, 0.1f, Time.deltaTime);
        }

        public void HandleCrouch(bool isCrouching)
        {
            _animator.SetBool(_crouchHash, isCrouching);
        }

        public void HandleJump()
        {
            _animator.SetTrigger(_jumpHash);
        }

        public void ResetJumpTrigger()
        {
            _animator.ResetTrigger(_jumpHash);
        }

        public void SetAnimationGrounding(bool isGrounded)
        {
            _animator.SetBool(_fallingHash, !isGrounded);
            _animator.SetBool(_groundHash, isGrounded);
        }

        public void SetFallingVelocity(float velocityY)
        {
            _animator.SetFloat(_zVelHash, velocityY, 0.1f, Time.deltaTime);
        }
    }
}