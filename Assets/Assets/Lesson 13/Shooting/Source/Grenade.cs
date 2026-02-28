using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Shooting
{
    public class Grenade
    {
        private static float GetReferenceArea(Rigidbody rigidbody)
        {
            Collider[] colliders = rigidbody.GetComponentsInChildren<Collider>();
            Bounds bounds = colliders[0].bounds;
            for (int i = 0; i < colliders.Length; ++i)
                bounds.Encapsulate(colliders[i].bounds);
            float width = bounds.size.x;
            float height = bounds.size.y;
            float length = bounds.size.z;
            return (width * height + width * length + height * length) / 3f;
        }

        public event Action<Grenade> OnExplode;
        public event Action<Collider[]> OnExplosionHit;
        
        private Rigidbody _rigidbody;
        private float _fuseTime;
        private float _explosionDelay;
        private float _explosionForce;
        private float _explosionRadius;
        private LayerMask _layerMask;
        private ExplosionController _explosionParticles;

        public float ExplosionRadius => _explosionRadius;
        public Vector3 Position => _rigidbody.position;
        
        public Grenade(GrenadeAction grenadeAction, Rigidbody grenadePrefab, Transform spawnPoint, Transform throwAnchor,
            float fuseTime, float explosionDelay, float throwForce, float torqueForce,
            float explosionForce, float explosionRadius, LayerMask layerMask, ExplosionController explosionParticles)
        {
            _fuseTime = fuseTime;
            _explosionDelay = explosionDelay;
            _explosionForce = explosionForce;
            _explosionRadius = explosionRadius;
            _layerMask = layerMask;
            _explosionParticles = explosionParticles;
            
            Vector3 position = spawnPoint.position;
            Quaternion rotation = spawnPoint.rotation;
            Vector3 direction = throwAnchor.forward;
            Vector3 force = direction * throwForce;
            Vector3 torque = new Vector3(
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce));
            _rigidbody = Object.Instantiate(grenadePrefab, position, rotation);
            _rigidbody.AddForce(force, ForceMode.Impulse);
            _rigidbody.AddTorque(torque, ForceMode.Impulse);
            
            grenadeAction.ExecuteCoroutine(FuseRoutine());
        }

        private IEnumerator FuseRoutine()
        {
            yield return new WaitForSeconds(_fuseTime);

            ExplosionController explosion = Object.Instantiate(_explosionParticles, _rigidbody.position, Quaternion.identity);
            explosion.ApplyRadius(_explosionRadius * 0.5f);
            explosion.Play();
            
            yield return new WaitForSeconds(_explosionDelay);

            ApplyForce();
            OnExplode?.Invoke(this);
            
            Object.Destroy(_rigidbody.gameObject);
        }

        private void ApplyForce()
        {
            Collider[] colliders = Physics.OverlapSphere(_rigidbody.position, _explosionRadius, _layerMask);
            
            OnExplosionHit?.Invoke(colliders);
            
            HashSet<Rigidbody> rigidbodies = new();
            for (int i = 0; i < colliders.Length; ++i)
                rigidbodies.Add(colliders[i].attachedRigidbody);
            foreach (Rigidbody rigidbody in rigidbodies)
            {
                if (rigidbody == null || _rigidbody == rigidbody)
                    continue;
                float referenceArea = GetReferenceArea(rigidbody);
                Vector3 direction = rigidbody.position - _rigidbody.position;
                float falloff = Mathf.Clamp01(1f - direction.magnitude / _explosionRadius);
                Vector3 force = _explosionForce * falloff * referenceArea * direction.normalized;
                rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}