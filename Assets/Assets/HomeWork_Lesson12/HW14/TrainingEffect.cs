using UnityEngine;

public class TrainingEffect : MonoBehaviour, IHitEffectReceiver
{
    [Header("Laser Hit")]
    [SerializeField] private GameObject _hitEffectPrefab;

    [Header("Magic Hit")]
    [SerializeField] private GameObject _magicHitEffectPrefab;

    [SerializeField] private float _effectHeightOffset = 1f;
    [SerializeField] private float _effectLifeTime = 2f;

    public void PlayHitEffect(Vector3 fromPosition)
    {
        PlayEffect(_hitEffectPrefab, fromPosition);
    }

    public void PlayMagicHitEffect(Vector3 fromPosition)
    {
        PlayEffect(_magicHitEffectPrefab, fromPosition);
    }

    private void PlayEffect(GameObject effectPrefab, Vector3 fromPosition)
    {
        if (effectPrefab == null)
            return;

        Vector3 hitPoint = transform.position + Vector3.up * _effectHeightOffset;

        GameObject effect = Instantiate(
            effectPrefab,
            hitPoint,
            Quaternion.LookRotation(hitPoint - fromPosition));

        Destroy(effect, _effectLifeTime);
    }
}