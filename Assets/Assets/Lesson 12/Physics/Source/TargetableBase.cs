using UnityEngine;

namespace PhysX
{
    public class TargetableBase : MonoBehaviour, ITargetable
    {
        [SerializeField] private Transform _targetPivot;
        
        public Transform TargetPivot => _targetPivot;
    }
}