using UnityEngine;
using Fiz;

public class ShieldShockwaveOnDeath : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private GameObject _shockwavePrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _lifeTime = 3f;

    private void Awake()
    {
        if (_health != null)
            _health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (_shockwavePrefab == null)
            return;

        Transform point = _spawnPoint != null ? _spawnPoint : transform;

        GameObject shockwave = Instantiate(
            _shockwavePrefab,
            point.position,
            point.rotation);

        Destroy(shockwave, _lifeTime);
    }
}