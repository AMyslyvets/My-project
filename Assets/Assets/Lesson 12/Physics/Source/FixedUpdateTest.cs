using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysX
{
    public class FixedUpdateTest : MonoBehaviour
    {
        [SerializeField] private int _frameRate;
        
        private int _fixedUpdateCount = 0;
        private bool _needClearCounter;
        private bool _isFirstFixedUpdate = true;

        private void Awake()
        {
            Application.targetFrameRate = _frameRate;
        }

        private void Update()
        {
            Debug.Log($"fixedUpdateCount: {_fixedUpdateCount}");
        }

        private void LateUpdate()
        {
            _isFirstFixedUpdate = true;
            
            /*if (_needClearCounter)
                _fixedUpdateCount = 0;
            _needClearCounter = false;*/
        }

        private void FixedUpdate()
        {
            if (_isFirstFixedUpdate)
            {
                _fixedUpdateCount = 1;
                _isFirstFixedUpdate = false;
            }
            else
            {
                _fixedUpdateCount++;
            }
        }
    }
}