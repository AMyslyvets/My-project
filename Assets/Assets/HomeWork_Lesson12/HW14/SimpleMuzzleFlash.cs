using UnityEngine;

namespace Fiz
{
    public class SimpleMuzzleFlash : MonoBehaviour
    {
        [SerializeField] private GameObject _muzzleFlashPrefab;
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private float _duration = 0.1f;

        private void OnEnable()
        {
            InputController.OnPrimaryInput += HandlePrimaryFire;
        }

        private void OnDisable()
        {
            InputController.OnPrimaryInput -= HandlePrimaryFire;
        }

        private void HandlePrimaryFire()
        {
            if (_muzzleFlashPrefab == null || _muzzlePoint == null)
                return;

            GameObject flash = Instantiate(_muzzleFlashPrefab, _muzzlePoint.position, _muzzlePoint.rotation, _muzzlePoint);
            Destroy(flash, _duration);
        }
    }
}