using UnityEngine;

namespace PhysX
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private TurretAnimator _turretAnimator;
        [SerializeField] private TurretTargeter _turretTargeter;

        private ITargetable _target;
        
        private void Start()
        {
            _turretTargeter.OnFirstTargetEnter += FirstTargetEnterHandler;
            _turretTargeter.OnLastTargetExit += LastTargetExitHandler;
        }

        private void FirstTargetEnterHandler(ITargetable target)
        {
            _target = target;
            StartCoroutine(_turretAnimator.Open());
        }

        private void LastTargetExitHandler(ITargetable target)
        {
            _target = null;
            StartCoroutine(_turretAnimator.Close());
        }

        private void Update()
        {
            if (_target == null)
                return;
            _turretAnimator.Aim(_target.TargetPivot.position, Time.deltaTime);
        }
    }
}