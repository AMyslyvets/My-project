using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lesson7
{
    public class InputController : MonoBehaviour
    {
        public static event Action<Vector2> OnMoveInput;
        public static event Action<Vector2> OnLookInput;
        public static event Action<bool> OnCameraLock;

        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private string _mapName;
        [SerializeField] private string _moveName;
        [SerializeField] private string _lookAroundName;
        [SerializeField] private string _cameraLockName;

        private InputAction _moveAction;
        private InputAction _lookAroundAction;
        private InputAction _cameraLockAction;

        private bool _inputUpdated;

        private void OnEnable()
        {
            _inputActionAsset.Enable();
            InputActionMap actionMap = _inputActionAsset.FindActionMap(_mapName);
            _moveAction = actionMap[_moveName];
            _lookAroundAction = actionMap[_lookAroundName];
            _cameraLockAction = actionMap[_cameraLockName];

            _moveAction.performed += MovePerformedHandler;
            _moveAction.canceled += MoveCanceledHandler;

            _lookAroundAction.performed += LookPerformedHandler;
            _lookAroundAction.canceled += LookCanceledHandler;
            
            _cameraLockAction.performed += CameraLockPerformedHandler;
            _cameraLockAction.canceled += CameraLockCanceledHandler;
        }

        private void OnDisable()
        {
            _inputActionAsset.Disable();
        }

        private void OnDestroy()
        {
            OnMoveInput = null;
            OnLookInput = null;
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
        
        private void LookCanceledHandler(InputAction.CallbackContext context)
        {
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
        }

        private void CameraLockPerformedHandler(InputAction.CallbackContext context)
        {
            OnCameraLock?.Invoke(true);
        }

        private void CameraLockCanceledHandler(InputAction.CallbackContext context)
        {
            OnCameraLock?.Invoke(false);
        }
    }
}