using UnityEngine;
using Fiz;

public class ShieldHealth : MonoBehaviour
{
    [SerializeField] private ShieldOrbit _shieldOrbit;
    [SerializeField] private Health _health;

    void Awake()
    {
        _health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        _health.OnDeath -= HandleDeath;
    }

    public void TakeHit(Vector3 hitPoint, int damage)
    {
        if (!_health.IsAlive) return;
        _shieldOrbit.OnHit(hitPoint);
        _health.TakeDamage(damage);
    }

    void HandleDeath()
    {
        _shieldOrbit.OnBreak();
    }
}