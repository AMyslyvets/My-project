using System;
using UnityEngine;

namespace Fiz
{
    public class MomentumDecayController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _momentumDecay;
        [SerializeField] private float _angularMomentumDecay;
        
        /*private Vector3 _collisionVelocity;
        private Vector3 _velocity;*/

        private int _collisionCounter = 0;
        
        private void OnCollisionEnter(Collision other)
        {
            _collisionCounter++;
            if (_collisionCounter <= 1)
                return;
            
            _rigidbody.velocity *= _momentumDecay;
            _rigidbody.angularVelocity *= _angularMomentumDecay;
        }

        /*private void FixedUpdate()
        {
            _velocity = _rigidbody.velocity;
        }*/

        /*private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_rigidbody.position, _rigidbody.position + _collisionVelocity.normalized);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_rigidbody.position, _rigidbody.position + _velocity.normalized);
        }*/
    }
}