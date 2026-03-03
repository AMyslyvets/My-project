using UnityEngine;

namespace Fiz
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private float _speed = 6f;

        private Transform _transform;
        private Vector2 _moveInput;

        private void Awake()
        {
            _transform = transform;
        }

        private void OnEnable()
        {
            InputController.OnMoveInput += MoveHandler;
        }

        private void OnDisable()
        {
            InputController.OnMoveInput -= MoveHandler;
            _moveInput = Vector2.zero;
        }

        private void Update()
        {
            if (_characterController == null) return;

            Vector3 forward = _transform.forward;
            Vector3 right = _transform.right;

            Vector3 movement = forward * _moveInput.y + right * _moveInput.x;
            _characterController.SimpleMove(movement * _speed);
        }

        private void MoveHandler(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }
    }
}