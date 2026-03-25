using UnityEngine;

namespace Fiz
{
    public class ProjectileGun : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _projectileSpeed = 20f;
        [SerializeField] private float _projectileLifeTime = 3f;

        private void OnEnable()
        {
            InputController.OnProjectileInput += ShootProjectile;
        }

        private void OnDisable()
        {
            InputController.OnProjectileInput -= ShootProjectile;
        }

        private void ShootProjectile()
        {
            if (_projectilePrefab == null || _firePoint == null)
                return;

            GameObject projectile = Instantiate(
                _projectilePrefab,
                _firePoint.position,
                _firePoint.rotation);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = _firePoint.forward * _projectileSpeed;
            }

            Destroy(projectile, _projectileLifeTime);
        }
    }
}