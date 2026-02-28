using UnityEngine;

namespace Lesson8
{
    public class BillboardBaker : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _treeRenderer;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _cameraRig;
        [SerializeField] private float[] _angles;
        [SerializeField] private Shader _colorShader;
        [SerializeField] private Shader _normalShader;
        
        public float[] Angles => _angles;
        public RenderTexture RenderTexture => _camera.targetTexture;
        public Transform CameraRig => _cameraRig;
        public Camera Camera => _camera;
        public Shader ColorShader => _colorShader;
        public Shader NormalShader => _normalShader;
    }
}