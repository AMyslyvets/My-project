using UnityEngine;

namespace PhysX
{
    public interface ITargetable
    {
        public Transform TargetPivot { get; }
    }
}