/*using System;
using Shooting;
using UnityEngine;

namespace Fiz
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
            Health health = collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(_damage);
            }
            if (_healthSystem.GetHealth(collider, out Health health))
                health.TakeDamage(_damage);
            OnHit?.Invoke(health ? 1 : 0);
        }
    }
}*/
using System;
using UnityEngine;

namespace Fiz
{
    public class GunDamageDealer : MonoBehaviour
    {
        public event Action<int> OnHit;

        [SerializeField] private HealthSystem _healthSystem;
        [SerializeField] private HitScanGun _gun;
        [SerializeField] private int _damage;
        [SerializeField] private ShieldOrbit _shieldOrbit;
        [SerializeField] private Health _health;

        public HitScanGun Gun => _gun;

        private void Start()
        {
            _gun.OnHit += GunHitHandler;
        }

        private void GunHitHandler(Collider collider)
        {
            Debug.Log($"Hit: {collider.gameObject.name}");
    
            if (_healthSystem.GetHealth(collider, out Health health))
            {
                Debug.Log($"Health found: {health.gameObject.name}");
                health.TakeDamage(_damage);
            }
            else
            {
                Debug.Log("Health NOT found");
            }

            // СНАЧАЛА пытаемся найти именно щит
            ShieldHitReceiver shield = collider.GetComponentInParent<ShieldHitReceiver>();

            if (shield != null)
            {
                shield.PlayHitEffect(_gun.transform.position);
            }
            else if (collider.GetComponentInParent<IHitEffectReceiver>() is IHitEffectReceiver receiver)
            {
                receiver.PlayHitEffect(_gun.transform.position);
            }
            else
            {
                Debug.Log("IHitEffectReceiver NOT found");
            }

            OnHit?.Invoke(health ? 1 : 0);
        }
    }
}