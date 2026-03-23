using System.Collections;
using UnityEngine;
using Fiz;

public class ShieldRegeneration : MonoBehaviour
{
    [SerializeField] private ShieldOrbit _shieldOrbit;
    [SerializeField] private Health _health;
    [SerializeField] private HexShieldBreak _hexShieldBreak;
    [SerializeField] private float _regenDelay = 5f;

    private bool _isRegenerating = false;

    void Awake()
    {
        _health.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        if (!_isRegenerating)
            StartCoroutine(RegenRoutine());
    }

    IEnumerator RegenRoutine()
    {
        _isRegenerating = true;

        yield return new WaitForSeconds(_regenDelay);

        _health.SetHealth(_health.MaxHealthValue);

        if (_hexShieldBreak != null)
            _hexShieldBreak.PlayAppear();

        _shieldOrbit.fixedShield = true;

        _isRegenerating = false;
    }
}