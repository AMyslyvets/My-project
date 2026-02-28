using System;
using System.Collections;
using UnityEngine;

namespace Fiz
{
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private float _regenerationDelayTime;
        
        private YieldInstruction _regenerationDelay;

        private void Start()
        {
            _regenerationDelay = new WaitForSeconds(_regenerationDelayTime);
            
            _health.OnDeath += DeathHandler;
        }

        private void DeathHandler()
        {
            StartCoroutine(RegenerationRoutine());
        }

        private IEnumerator RegenerationRoutine()
        {
            yield return _regenerationDelay;
            _health.SetHealth(_health.MaxHealthValue);
        }
    }
}
