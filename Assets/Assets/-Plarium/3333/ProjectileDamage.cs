using UnityEngine;

namespace Fiz
{
    public class ProjectileDamage : MonoBehaviour
    {
        [SerializeField] private int _damage = 10;
        [SerializeField] private LayerMask _hitMask = ~0;
        [SerializeField] private float _hitRadius = 0.35f;
        [SerializeField] private bool _destroyOnHit = true;

        private HealthSystem _healthSystem;
        private Vector3 _previousPosition;
        private bool _hasHit;

        private void Awake()
        {
            Debug.Log("ProjectileDamage Awake on: " + gameObject.name);

            _healthSystem = FindObjectOfType<HealthSystem>();
            _previousPosition = transform.position;
        }

        private void Update()
        {
            Debug.Log("ProjectileDamage Update on: " + gameObject.name);

            if (_hasHit)
                return;

            Vector3 currentPosition = transform.position;
            Vector3 move = currentPosition - _previousPosition;
            float distance = move.magnitude;

            if (distance > 0f)
            {
                Vector3 direction = move / distance;

                if (Physics.SphereCast(
                        _previousPosition,
                        _hitRadius,
                        direction,
                        out RaycastHit hitInfo,
                        distance,
                        _hitMask,
                        QueryTriggerInteraction.Collide))
                {
                    Debug.Log("Projectile SphereCast hit: " + hitInfo.collider.name);
                    ProcessHit(hitInfo.collider, hitInfo.point);
                }
            }

            _previousPosition = currentPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Projectile OnTriggerEnter with: " + other.name);

            if (_hasHit)
                return;

            if (((1 << other.gameObject.layer) & _hitMask) == 0)
                return;

            ProcessHit(other, transform.position);
        }

        private void ProcessHit(Collider other, Vector3 hitPoint)
        {
            Debug.Log("Projectile ProcessHit with: " + other.name);

            if (_hasHit)
                return;

            bool hasHitSomething = false;

            if (other.GetComponentInParent<Health>() is Health directHealth)
            {
                Debug.Log("Projectile found direct Health on: " + directHealth.name);
                directHealth.TakeDamage(_damage);
                hasHitSomething = true;
            }
            else if (_healthSystem != null && _healthSystem.GetHealth(other, out Health health))
            {
                Debug.Log("Projectile found Health through HealthSystem on: " + health.name);
                health.TakeDamage(_damage);
                hasHitSomething = true;
            }

            if (other.GetComponentInParent<TrainingEffect>() is TrainingEffect trainingEffect)
            {
                trainingEffect.PlayMagicHitEffect(hitPoint);
                hasHitSomething = true;
            }
            /* else if (other.GetComponentInParent<IHitEffectReceiver>() is IHitEffectReceiver receiver)
            {
                receiver.PlayHitEffect(hitPoint);
                hasHitSomething = true;
            } */

            if (!hasHitSomething)
            {
                Debug.Log("Projectile hit something, but found no Health and no IHitEffectReceiver");
                return;
            }

            _hasHit = true;

            if (_destroyOnHit)
                Destroy(gameObject);
        }
    }
}