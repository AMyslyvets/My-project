using UnityEngine;
using Fiz;

public class ShieldHitReceiver : MonoBehaviour, IHitEffectReceiver
{
    [SerializeField] private ShieldOrbit _shieldOrbit;
    [SerializeField] private Health _health;

    void Awake()
    {
        _health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (_health != null) _health.OnDeath -= HandleDeath;
    }

    public void PlayHitEffect(Vector3 fromPosition)
    {
        Debug.Log($"Shield hit! ShieldOrbit null: {_shieldOrbit == null}");
        _shieldOrbit.OnHit(fromPosition);
    }

    void HandleDeath()
    {
        Debug.Log("Shield HandleDeath called");
        _shieldOrbit.OnBreak();
    }
    
}