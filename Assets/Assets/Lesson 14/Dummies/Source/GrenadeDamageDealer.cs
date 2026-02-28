using System;
using Shooting;
using UnityEngine;

namespace Dummies
{
    public class GrenadeDamageDealer : MonoBehaviour
    {
        public event Action<int> OnHit;

        [SerializeField] private HealthSystem _healthSystem;
        [SerializeField] private GrenadeAction _grenadeAction;
        [SerializeField] private int _damage;

        private void Start()
        {
            _grenadeAction.OnGrenadeSpawned += GrenadeSpawnedHandler;
        }

        private void GrenadeSpawnedHandler(Grenade grenade)
        {
            grenade.OnExplosionHit += colliders => GrenadeExplosionHitHandler(grenade, colliders);
        }

        private void GrenadeExplosionHitHandler(Grenade grenade, Collider[] colliders)
        {
            int hitCount = 0;
            for (int i = 0; i < colliders.Length; ++i)
            {
                Collider collider = colliders[i];
                if (_healthSystem.GetHealth(collider, out Health health))
                {
                    Vector3 direction = collider.transform.position - grenade.Position;
                    float falloff = Mathf.Clamp01(1f - direction.magnitude / grenade.ExplosionRadius);
                    health.TakeDamage((int)(_damage * falloff));
                    hitCount++;
                }
            }
            OnHit?.Invoke(hitCount);
        }
    }
}