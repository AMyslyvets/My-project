using UnityEngine;

namespace Fiz
{
    public interface ITargetable
    {
        public Transform TargetPivot { get; }
    }
}