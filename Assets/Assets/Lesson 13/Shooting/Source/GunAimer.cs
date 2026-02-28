using Cinemachine;
using UnityEngine;

namespace Shooting
{
    public class GunAimer : MonoBehaviour
    {
        [SerializeField] private Transform _gunTransform;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _rayDistance;
        [SerializeField] private LayerMask _rayMask;
        [SerializeField] private CinemachineMixingCamera _mixingCamera;
        [SerializeField] private float _aimSpeed;

        private Vector3 _hitPoint;
        private Vector3 _collisionPoint;
        private float _aimValue;
        private float _targetAimValue;

        public Vector3 AimPoint => _hitPoint;
        public Vector3 CollisionPoint => _collisionPoint;
        
        private void OnEnable()
        {
            PhysX.InputController.OnSecondaryInput += SecondaryInputHandler;
        }

        private void OnDisable()
        {
            PhysX.InputController.OnSecondaryInput -= SecondaryInputHandler;
        }

        private void FixedUpdate()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            _hitPoint = _cameraTransform.position + _cameraTransform.forward * _rayDistance;
            _collisionPoint = _hitPoint;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _rayDistance, _rayMask))
                _hitPoint = hitInfo.point;
            _gunTransform.LookAt(_hitPoint);
            
            if (Physics.Raycast(_gunTransform.position, _gunTransform.forward, out RaycastHit collisionInfo, _rayDistance,
                    _rayMask, QueryTriggerInteraction.Ignore))
            {
                _collisionPoint = collisionInfo.point;
            }
        }

        private void Update()
        {
            _aimValue = Mathf.MoveTowards(_aimValue, _targetAimValue, _aimSpeed * Time.deltaTime);
            _mixingCamera.m_Weight0 = 1f - _aimValue;
            _mixingCamera.m_Weight1 = _aimValue;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_gunTransform.position, _hitPoint);
        }

        private void SecondaryInputHandler(bool performed)
        {
            _targetAimValue = performed ? 1f : 0f;
        }
    }
}