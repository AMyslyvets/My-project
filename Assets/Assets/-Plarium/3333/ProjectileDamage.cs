using UnityEngine;

namespace Fiz
{
    public class ProjectileDamage : MonoBehaviour
    {
        [SerializeField] private int _damage = 10;
        [SerializeField] private LayerMask _hitMask = ~0;
        [SerializeField] private bool _destroyOnHit = true;

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & _hitMask) == 0)
                return;

            if (other.attachedRigidbody != null && other.attachedRigidbody.gameObject == gameObject)
                return;

            if (other.GetComponentInParent<Health>() is Health directHealth)
            {
                directHealth.TakeDamage(_damage);
            }
            else if (FindObjectOfType<HealthSystem>() is HealthSystem healthSystem &&
                     healthSystem.GetHealth(other, out Health health))
            {
                health.TakeDamage(_damage);
            }

            if (_destroyOnHit)
                Destroy(gameObject);
        }
    }
}