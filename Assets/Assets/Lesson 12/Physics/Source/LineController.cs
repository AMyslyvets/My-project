using System.Collections;
using UnityEngine;

namespace PhysX
{
    public class LineController : MonoBehaviour
    {
        [SerializeField] private Destructor _destructor;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _muzzleTransform;
        [SerializeField] private float _rayDuration;
        
        private readonly Vector3[] _points = new Vector3[2];

        private Coroutine _coroutine;
        private float _rayLifetime;
        
        private void Start()
        {
            _destructor.OnPrimaryFire += PrimaryFireHandler;
        }

        private void PrimaryFireHandler(Vector3 point)
        {
            if (_coroutine == null)
            {
                _coroutine = StartCoroutine(RayRoutine(point));
            }
            else
            {
                _points[0] = _muzzleTransform.position;
                _points[1] = point;
                _lineRenderer.SetPositions(_points);
                _rayLifetime += _rayDuration;
            }
        }

        private IEnumerator RayRoutine(Vector3 point)
        {
            _points[0] = _muzzleTransform.position;
            _points[1] = point;
            _lineRenderer.SetPositions(_points);
            _lineRenderer.enabled = true;
            _rayLifetime = _rayDuration;

            while (_rayLifetime > 0f)
            {
                yield return null;
                _rayLifetime -= Time.deltaTime;
            }
            
            _lineRenderer.enabled = false;
            _coroutine = null;
        }
    }
}