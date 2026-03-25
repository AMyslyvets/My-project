using UnityEngine;

namespace Fiz
{
    public class ProjectileGun : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private GunAimer _aimer;
        [SerializeField] private float _speed = 20f;
        [SerializeField] private float _lifeTime = 3f;

        private void OnEnable()
        {
            InputController.OnProjectileInput += Shoot;
        }

        private void OnDisable()
        {
            InputController.OnProjectileInput -= Shoot;
        }

        private void Shoot()
        {
            if (_projectilePrefab == null || _firePoint == null || _aimer == null)
                return;

            Vector3 direction = (_aimer.AimPoint - _firePoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject projectile = Instantiate(
                _projectilePrefab,
                _firePoint.position,
                rotation);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = direction * _speed;
            }

            Destroy(projectile, _lifeTime);
        }
    }
}