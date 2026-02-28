using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fiz
{
    public class GrenadeAction : MonoBehaviour
    {
        public event Action<Grenade> OnGrenadeSpawned;
        
        [SerializeField] private Transform _throwAnchor;
        [SerializeField] private Transform _spawnAnchor;
        
        [SerializeField] private Rigidbody _grendePrefab;
        [SerializeField] private float _throwForce;
        [SerializeField] private float _throwTorque;
        [SerializeField] private float _fuseTime;
        [SerializeField] private float _explosionDelay;
        [SerializeField] private float _explosionForce;
        [SerializeField] private float _explosionRadius;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private ExplosionController _explosionParticles;
        [SerializeField] private bool _breakOnSpawn;
        
        private HashSet<Grenade> _grenades = new();
        
        private Vector3 _lastGrenadePosition;
        private float _lastGrenadeRadius;
        
        private void Start()
        {
            _explosionParticles.ApplyRadius(_explosionRadius);
        }

        private void OnEnable()
        {
           Fiz.InputController.OnGrenadeInput += GrenadeInputHandler;
        }
        
        private void OnDisable()
        {
           Fiz.InputController.OnGrenadeInput -= GrenadeInputHandler;
        }

        private void GrenadeInputHandler()
        {
            Grenade grenade = new Grenade(this, _grendePrefab, _spawnAnchor, _throwAnchor,
                _fuseTime, _explosionDelay, _throwForce, _throwTorque,
                _explosionForce, _explosionRadius, _layerMask, _explosionParticles);
            grenade.OnExplode += GrenadeExplodeHandler;
            _grenades.Add(grenade);
            
            OnGrenadeSpawned?.Invoke(grenade);
            
            if (_breakOnSpawn)
                Debug.Break();
        }

        public void ExecuteCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        private void GrenadeExplodeHandler(Grenade grenade)
        {
            _lastGrenadePosition = grenade.Position;
            _lastGrenadeRadius = grenade.ExplosionRadius;
            _grenades.Remove(grenade);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(_lastGrenadePosition, _lastGrenadeRadius);
        }
    }
}