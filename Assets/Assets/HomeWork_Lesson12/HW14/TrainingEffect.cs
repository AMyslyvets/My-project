using UnityEngine;

public class TrainingEffect : MonoBehaviour, IHitEffectReceiver
{
    [SerializeField] private GameObject _hitEffectPrefab; // Префаб визуального эффекта
    [SerializeField] private float _effectHeightOffset = 1f;

    public void PlayHitEffect(Vector3 fromPosition)
    {
        if (_hitEffectPrefab == null)
            return;

        // Определяем точку появления эффекта (в центре объекта с небольшим смещением вверх)
        Vector3 hitPoint = transform.position + Vector3.up * _effectHeightOffset;

        // Создаем эффект
        GameObject effect = Instantiate(_hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitPoint - fromPosition));
        Destroy(effect, 2f); // удаляем эффект через 2 секунды
    }
}