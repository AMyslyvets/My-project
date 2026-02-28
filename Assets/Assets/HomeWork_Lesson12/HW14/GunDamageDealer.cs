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

        public HitScanGun Gun => _gun;

        private void Start()
        {
            _gun.OnHit += GunHitHandler;
        }

        private void GunHitHandler(Collider collider)
        {
            // Получаем Health
            if (_healthSystem.GetHealth(collider, out Health health))
            {
                health.TakeDamage(_damage);
            }

            // Воспроизводим hit effect (если цель его поддерживает)
            if (collider.TryGetComponent<IHitEffectReceiver>(out var receiver))
            {
                receiver.PlayHitEffect(_gun.transform.position); // можно передать точку выстрела или попадания
            }

            OnHit?.Invoke(health ? 1 : 0);
        }
    }
}