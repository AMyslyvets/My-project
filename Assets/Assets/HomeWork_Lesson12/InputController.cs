using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Fiz
{
    public class InputController : MonoBehaviour
    {
        public static event Action<Vector2> OnMoveInput;
        public static event Action<Vector2> OnLookInput;
        public static event Action OnPrimaryInput;
        public static event Action<bool> OnSecondaryInput;
        public static event Action OnGrenadeInput;
        public static event Action<bool> OnScoreInput;
        public static event Action OnReload;
        public static event Action OnEscape;
        public static event Action OnJump;
        public static event Action OnBank;

        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private string _mapName;
        [SerializeField] private string _UImapName;
        [SerializeField] private string _moveName;
        [SerializeField] private string _lookAroundName;
        [SerializeField] private string _pointerPositionName;
        [SerializeField] private string _primaryFireName;
        [SerializeField] private string _secondaryFireName;
        [SerializeField] private string _grenadeName;
        [SerializeField] private string _scoreName;
        [SerializeField] private string _reloadName;
        [SerializeField] private string _escapeName;
        [SerializeField] private string _jumpName;
        [SerializeField] private string _bankName;
        [SerializeField] private CursorLockMode _enabledCursorMode;
        [SerializeField] private CursorLockMode _disabledCursorMode;

        private InputAction _moveAction;
        private InputAction _lookAroundAction;
        private InputAction _pointerPositionAction;
        private InputAction _primaryFireAction;
        private InputAction _secondaryFireAction;
        private InputAction _grenadeAction;
        private InputAction _scoreAction;
        private InputAction _reloadAction;
        private InputAction _escapeAction;
        private InputAction _jumpAction;
        private InputAction _bankAction;

        private bool _inputUpdated;

        private InputActionMap _actionMap;
        private InputActionMap _gameplayUIActionMap;

        private void OnEnable()
        {
            Cursor.visible = false;
            Cursor.lockState = _enabledCursorMode;

            _inputActionAsset.Enable();

            _actionMap = _inputActionAsset.FindActionMap(_mapName);
            _gameplayUIActionMap = _inputActionAsset.FindActionMap(_UImapName);

            _moveAction = _actionMap[_moveName];
            _lookAroundAction = _actionMap[_lookAroundName];
            //_pointerPositionAction = _actionMap[_pointerPositionName];
            _primaryFireAction = _actionMap[_primaryFireName];
            _secondaryFireAction = _actionMap[_secondaryFireName];
            _grenadeAction = _actionMap[_grenadeName];
            _scoreAction = _actionMap[_scoreName];
            _reloadAction = _actionMap[_reloadName];
            _jumpAction = _actionMap[_jumpName];
            _escapeAction = _gameplayUIActionMap[_escapeName];
            _bankAction = _actionMap[_bankName];

            _moveAction.performed += MovePerformedHandler;
            _moveAction.canceled += MoveCanceledHandler;

            _lookAroundAction.performed += LookPerformedHandler;
            _lookAroundAction.canceled += LookPerformedHandler;

            _primaryFireAction.performed += PrimaryFirePerformedHandler;

            _secondaryFireAction.performed += SecondaryFirePerformedHandler;
            _secondaryFireAction.canceled += SecondaryFireCanceledHandler;

            _grenadeAction.performed += GrenadePerformedHandler;

            _scoreAction.performed += ScorePerformedHandler;
            _scoreAction.canceled += ScoreCanceledHandler;

            _reloadAction.performed += ReloadPerformedHandler;

            _jumpAction.performed += JumpPerformedHandler;

            _escapeAction.performed += EscapePerformedHandler;
            _bankAction.performed += BankPerformedHandler;
        }

        private void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = _disabledCursorMode;

            if (_actionMap != null)
                _actionMap.Disable();

            if (_gameplayUIActionMap != null)
                _gameplayUIActionMap.Disable();
        }

        private void OnDestroy()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= MovePerformedHandler;
                _moveAction.canceled -= MoveCanceledHandler;
            }

            if (_lookAroundAction != null)
            {
                _lookAroundAction.performed -= LookPerformedHandler;
                _lookAroundAction.canceled -= LookPerformedHandler;
            }

            if (_primaryFireAction != null)
                _primaryFireAction.performed -= PrimaryFirePerformedHandler;

            if (_secondaryFireAction != null)
            {
                _secondaryFireAction.performed -= SecondaryFirePerformedHandler;
                _secondaryFireAction.canceled -= SecondaryFireCanceledHandler;
            }

            if (_grenadeAction != null)
                _grenadeAction.performed -= GrenadePerformedHandler;

            if (_scoreAction != null)
            {
                _scoreAction.performed -= ScorePerformedHandler;
                _scoreAction.canceled -= ScoreCanceledHandler;
            }

            if (_reloadAction != null)
                _reloadAction.performed -= ReloadPerformedHandler;

            if (_jumpAction != null)
                _jumpAction.performed -= JumpPerformedHandler;

            if (_escapeAction != null)
                _escapeAction.performed -= EscapePerformedHandler;

            if (_bankAction != null)
                _bankAction.performed -= BankPerformedHandler;

            OnMoveInput = null;
            OnLookInput = null;
            OnPrimaryInput = null;
            OnSecondaryInput = null;
            OnGrenadeInput = null;
            OnScoreInput = null;
            OnReload = null;
            OnEscape = null;
            OnJump = null;
            OnBank = null;
        }

        private void MovePerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnMoveInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void MoveCanceledHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnMoveInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void LookPerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void PrimaryFirePerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnPrimaryInput?.Invoke();
        }

        private void SecondaryFirePerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnSecondaryInput?.Invoke(true);
        }

        private void SecondaryFireCanceledHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnSecondaryInput?.Invoke(false);
        }

        private void GrenadePerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnGrenadeInput?.Invoke();
        }

        private void ScorePerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnScoreInput?.Invoke(true);
        }

        private void ScoreCanceledHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnScoreInput?.Invoke(false);
        }

        private void ReloadPerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnReload?.Invoke();
        }

        private void JumpPerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened) return;
            OnJump?.Invoke();
        }

        private void EscapePerformedHandler(InputAction.CallbackContext context)
        {
            if (UIState.IsWindowOpened)
            {
                OnEscape?.Invoke();
                return;
            }

            SceneManager.LoadScene("FizLobby", LoadSceneMode.Single);
        }

        private void BankPerformedHandler(InputAction.CallbackContext context)
        {
            OnBank?.Invoke();
        }
    }
}