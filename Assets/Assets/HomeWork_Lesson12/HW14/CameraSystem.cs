using UnityEngine;

namespace Fiz
{
    public class CameraSystem : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        
        public Camera Camera => _camera;
    }
}