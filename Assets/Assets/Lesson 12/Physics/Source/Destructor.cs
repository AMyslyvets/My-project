using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysX
{
    public class Destructor : MonoBehaviour
    {
        public Action<Vector3> OnPrimaryFire;

        [SerializeField] private DestructableSystem _destructableSystem;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _rayDistance;
        [SerializeField] private LayerMask _rayMask;
        [SerializeField] private LayerMask _explsionMask;

        [SerializeField] private ExplosionController _explosionController;
        [SerializeField] private float _explosionRadius;
        [SerializeField] private float _explosionForce;
        [SerializeField] private float _verticalOffset;

        private float _radiusReciprocal;
        
        private void Start()
        {
            InputController.OnPrimaryInput += PrimaryFireHandler;
            InputController.OnSecondaryInput += SecondaryFireHandler;
        }

        private void OnEnable()
        {
            _radiusReciprocal = 1f / _explosionRadius;
            _explosionController.ApplyRadius(_explosionRadius);
        }

        private void PrimaryFireHandler()
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            Vector3 _hitPoint = _cameraTransform.position + _cameraTransform.forward * _rayDistance;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _rayDistance, _rayMask))
            {
                _hitPoint = hitInfo.point;
                _destructableSystem.Destruct(hitInfo.rigidbody);
            }
            OnPrimaryFire?.Invoke(_hitPoint);
        }

        private void SecondaryFireHandler(bool performed)
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            Vector3 _hitPoint = _cameraTransform.position + _cameraTransform.forward * _rayDistance;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, _rayDistance, _rayMask))
            {
                _hitPoint = hitInfo.point;

                Collider[] colliders = Physics.OverlapSphere(_hitPoint, _explosionRadius, _explsionMask);

                HashSet<Rigidbody> _targets = new HashSet<Rigidbody>();
                
                for (int i = 0; i < colliders.Length; ++i)
                {
                    Rigidbody rigidbody = colliders[i].attachedRigidbody;
                    _ = _targets.Add(rigidbody);
                }

                foreach (Rigidbody rigidbody in _targets)
                {
                    if (rigidbody == null)
                        continue;
                    Vector3 direction = rigidbody.position - (_hitPoint + Vector3.up * _verticalOffset);
                    rigidbody.AddForce(
                        direction.normalized * _explosionForce * Mathf.Clamp01(1f - direction.magnitude * _radiusReciprocal),
                        ForceMode.Impulse);
                }
                
                Instantiate(_explosionController, _hitPoint, Quaternion.identity).Play();
            }
        }
    }
}