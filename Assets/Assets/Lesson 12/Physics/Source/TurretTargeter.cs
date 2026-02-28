using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysX
{
    public class TurretTargeter : MonoBehaviour
    {
        public Action<ITargetable> OnFirstTargetEnter;
        public Action<ITargetable> OnLastTargetExit;
        
        [SerializeField] private Transform _transform;
        
        private HashSet<ITargetable> _targets = new();
        
        private void OnTriggerEnter(Collider other)
        {
            ITargetable target = other.GetComponent<ITargetable>();
            if (target == null)
                return;
            _targets.Add(target);
            if (_targets.Count < 2)
                OnFirstTargetEnter?.Invoke(target);
        }

        private void OnTriggerExit(Collider other)
        {
            ITargetable target = other.GetComponent<ITargetable>();
            if (target == null)
                return;
            _targets.Remove(target);
            if (_targets.Count < 1)
                OnLastTargetExit?.Invoke(target);
        }

        public ITargetable GetTarget()
        {
            float minSqrDistance = float.MaxValue;
            ITargetable closest = null;
            foreach (ITargetable target in _targets)
            {
                float sqrDistance = (target.TargetPivot.position - _transform.position).sqrMagnitude;
                if (sqrDistance >= minSqrDistance)
                    continue;
                closest = target;
                minSqrDistance = sqrDistance;
            }
            return closest;
        }
    }
}