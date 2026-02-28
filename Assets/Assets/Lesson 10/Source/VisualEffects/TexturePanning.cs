using UnityEngine;

public class TexturePanning : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private float _offsetInterval;
    [SerializeField] private int _steps;
    [SerializeField] private string _offsetPropertyName;
    [SerializeField] private float _speed;
    //[SerializeField, Range(0, 16)] private int _currentStep;
    
    private int _offsetPropertyId;
    private Vector4 _offset;
    private float _time;
    
    private void Awake()
    {
        _offsetPropertyId = Shader.PropertyToID(_offsetPropertyName);
        _offset = _material.GetVector(_offsetPropertyId);
    }

    private void Update()
    {
        _offset.w = 1f - _offsetInterval * (int)(_time / _steps);
        _material.SetVector(_offsetPropertyId, _offset);
        _time += Time.deltaTime * _speed;
    }
}
