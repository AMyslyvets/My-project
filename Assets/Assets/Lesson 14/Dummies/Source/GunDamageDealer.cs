using System;
using Shooting;
using UnityEngine;

namespace Dummies
{
    public class GunDamageDealer : MonoBehaviour
    {
        public event Action<int> OnHit;
        
        [SerializeField] private HealthSystem _healthSystem;
        [SerializeField] private HitScanGun _gun;
        [SerializeField] private int _damage;

        public HitScanGun Gun => _gun;
        
        private void Start()
        {
            _gun.OnHit += GunHitHandler;
        }

        private void GunHitHandler(Collider collider)
        {
            if (_healthSystem.GetHealth(collider, out Health health))
                health.TakeDamage(_damage);
            OnHit?.Invoke(health ? 1 : 0);
        }
    }
}