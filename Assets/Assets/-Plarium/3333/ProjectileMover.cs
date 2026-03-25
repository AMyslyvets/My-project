using UnityEngine;

namespace Fiz
{
    public class ProjectileMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 20f;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _rb.velocity = transform.forward * _speed;
        }
    }
}