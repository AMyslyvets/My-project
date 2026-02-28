using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PhysX
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
            //_pointerPositionAction = actionMap[_pointerPositionName];
            _primaryFireAction = _actionMap[_primaryFireName];
            _secondaryFireAction = _actionMap[_secondaryFireName];
            _grenadeAction = _actionMap[_grenadeName];
            _scoreAction = _actionMap[_scoreName];
            _reloadAction = _actionMap[_reloadName];
            _jumpAction = _actionMap[_jumpName];
            _escapeAction = _gameplayUIActionMap[_escapeName];

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
        }

        private void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = _disabledCursorMode;
            
            _actionMap.Disable();
        }

        private void OnDestroy()
        {
            _moveAction.performed -= MovePerformedHandler;
            _moveAction.canceled -= MoveCanceledHandler;
        
            _lookAroundAction.performed -= LookPerformedHandler;

            _primaryFireAction.performed -= PrimaryFirePerformedHandler;
            
            _secondaryFireAction.performed -= SecondaryFirePerformedHandler;
            _secondaryFireAction.canceled -= SecondaryFireCanceledHandler;

            _grenadeAction.performed -= GrenadePerformedHandler;
            
            _scoreAction.performed -= ScorePerformedHandler;
            _scoreAction.canceled -= ScoreCanceledHandler;
            
            _reloadAction.performed -= ReloadPerformedHandler;
            
            _escapeAction.performed -= EscapePerformedHandler;
            
            OnMoveInput = null;
            OnLookInput = null;
            OnPrimaryInput = null;
            OnSecondaryInput = null;
            OnGrenadeInput = null;
            OnScoreInput = null;
            OnReload = null;
            OnEscape = null;
        }

        private void MovePerformedHandler(InputAction.CallbackContext context)
        {
            OnMoveInput?.Invoke(context.ReadValue<Vector2>());
        }
    
        private void MoveCanceledHandler(InputAction.CallbackContext context)
        {
            OnMoveInput?.Invoke(context.ReadValue<Vector2>());
        }
    
        private void LookPerformedHandler(InputAction.CallbackContext context)
        {
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void PrimaryFirePerformedHandler(InputAction.CallbackContext context)
        {
            OnPrimaryInput?.Invoke();
        }
        
        private void SecondaryFirePerformedHandler(InputAction.CallbackContext context)
        {
            OnSecondaryInput?.Invoke(true);
        }
        
        private void SecondaryFireCanceledHandler(InputAction.CallbackContext context)
        {
            OnSecondaryInput?.Invoke(false);
        }
        
        private void GrenadePerformedHandler(InputAction.CallbackContext context)
        {
            OnGrenadeInput?.Invoke();
        }
        
        private void ScorePerformedHandler(InputAction.CallbackContext context)
        {
            OnScoreInput?.Invoke(true);
        }
        
        private void ScoreCanceledHandler(InputAction.CallbackContext context)
        {
            OnScoreInput?.Invoke(false);
        }

        private void ReloadPerformedHandler(InputAction.CallbackContext context)
        {
            OnReload?.Invoke();
        }
        
        private void JumpPerformedHandler(InputAction.CallbackContext context)
        {
            OnJump?.Invoke();
        }
        
        private void EscapePerformedHandler(InputAction.CallbackContext context)
        {
            OnEscape?.Invoke();
        }
    }
}