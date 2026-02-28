using UnityEngine;

namespace Lesson7
{
    public class CameraLocker : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private Transform _regularYawAnchor;
        [SerializeField] private Transform _lockedYawAnchor;
        
        private void Start()
        {
            InputController.OnCameraLock += CameraLockHandler;
        }

        public void Lock(bool isLocked)
        {
            _cameraController.SetYawAnchor(isLocked ? _lockedYawAnchor : _regularYawAnchor);
        }
        
        private void CameraLockHandler(bool isLocked)
        {
            Lock(isLocked);
        }
    }
}