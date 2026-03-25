using UnityEngine;
using Fiz;

public class ShieldHitReceiver : MonoBehaviour, IHitEffectReceiver
{
    [SerializeField] private ShieldOrbit _shieldOrbit;
    [SerializeField] private Health _health;

    [Header("Laser hit effect")]
    [SerializeField] private GameObject _hitEffectPrefab;
    [SerializeField] private float _effectHeightOffset = 1f;
    [SerializeField] private float _effectLifeTime = 2f;

    void Awake()
    {
        _health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    public void PlayHitEffect(Vector3 fromPosition)
    {
        _shieldOrbit.OnHit(fromPosition);

        if (_hitEffectPrefab == null)
            return;

        Vector3 hitPoint = transform.position + Vector3.up * _effectHeightOffset;

        GameObject effect = Instantiate(
            _hitEffectPrefab,
            hitPoint,
            Quaternion.LookRotation(hitPoint - fromPosition));

        Destroy(effect, _effectLifeTime);
    }

    void HandleDeath()
    {
        _shieldOrbit.OnBreak();
    }
}