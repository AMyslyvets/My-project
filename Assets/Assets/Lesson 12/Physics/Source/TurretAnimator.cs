using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysX
{
    public class TurretAnimator : MonoBehaviour
    {
        [SerializeField] private Transform _legsJoint;
        [SerializeField] private Transform _barrelJoint;
        [SerializeField] private Transform _aimJoint;
        [SerializeField] private Transform _aimPivot;

        [SerializeField, Range(0f, 1f)] private float _legsHeight;
        //[SerializeField, Range(-1f, 1f)] private float _pitch;
        //[SerializeField, Range(0f, 1f)] private float _yaw;
        [SerializeField, Range(0f, 1f)] private float _barrelOpen;
        [SerializeField] private Vector2 _heightBounds;
        [SerializeField] private Vector2 _pitchBounds;
        [SerializeField] private Vector2 _yawBounds;
        [SerializeField] private Vector2 _barrelBounds;
        [SerializeField] private float _barrelCloseDuration;
        [SerializeField] private float _legsCloseDuration;
        [SerializeField] private Vector2 _aimSpeed;

        private void Update()
        {
            Vector3 legPosition = new Vector3(0f, Mathf.Lerp(_heightBounds.x, _heightBounds.y, _legsHeight), 0f);
            _legsJoint.localPosition = legPosition;
            _barrelJoint.localRotation = Quaternion.Euler(Mathf.Lerp(_barrelBounds.x, _barrelBounds.y, _barrelOpen), 0f, 0f);
        }

        
        public IEnumerator Open()
        {
            float time = 0f;
            float reciprocal = 1f / _legsCloseDuration;
            while (time < _legsCloseDuration)
            {
                _legsHeight = time * reciprocal;
                yield return null;
                time += Time.deltaTime;
            }

            time = 0f;
            reciprocal = 1f / _barrelCloseDuration;
            while (time < _barrelCloseDuration)
            {
                _barrelOpen = time * reciprocal;
                yield return null;
                time += Time.deltaTime;
            }
        }
        
        public IEnumerator Close()
        {
            float time = 0f;
            float reciprocal = 1f / _barrelCloseDuration;
            float pitch = _aimJoint.localEulerAngles.x;
            while (time < _barrelCloseDuration)
            {
                float progress = time * reciprocal;
                Vector3 aimRotation = _aimJoint.localEulerAngles;
                aimRotation.x = Mathf.Lerp(pitch, 0f, progress);
                _aimJoint.localEulerAngles = aimRotation;
                _barrelOpen = 1f - progress;
                yield return null;
                time += Time.deltaTime;
            }

            time = 0f;
            reciprocal = 1f / _legsCloseDuration;
            while (time < _legsCloseDuration)
            {
                _legsHeight = 1f - time * reciprocal;
                yield return null;
                time += Time.deltaTime;
            }
        }

        public void Aim(Vector3 aimPosition, float deltaTime)
        {
            Vector3 targetForward = (aimPosition - _aimPivot.position).normalized;
            Vector3 targetForwardXZ = Vector3.ProjectOnPlane(targetForward, Vector3.up).normalized;
            
            Vector3 yawForward = Vector3.RotateTowards(_legsJoint.forward, targetForwardXZ, _aimSpeed.x * deltaTime, 0f);
            _legsJoint.rotation = Quaternion.LookRotation(yawForward, Vector3.up);
            
            Vector3 targetForwardYZ = Vector3.ProjectOnPlane(targetForward, _legsJoint.right).normalized;
            Vector3 pitchForward = Vector3.RotateTowards(_aimJoint.forward, targetForwardYZ, _aimSpeed.y * deltaTime, 0f);
            _aimJoint.rotation = Quaternion.LookRotation(pitchForward, Vector3.up);
        }
    }
}