using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson8
{
    public class TerrainGrassWind : MonoBehaviour
    {
        private const string WIND_POSITION_PROPERTY = "_WindPosition";
        private const string WIND_DIRECTION_PROPERTY = "_WindDirection";

        [SerializeField] private Material _material;
        [SerializeField] private Vector2 _windSpeed;
        [SerializeField] private Vector3 _windDirection;
        
        private Vector2 _windPosition;

        private int _windPositionId;
        private int _windDirectionId;

        private void OnEnable()
        {
            _windPositionId = Shader.PropertyToID(WIND_POSITION_PROPERTY);
            _windDirectionId = Shader.PropertyToID(WIND_DIRECTION_PROPERTY);
            _material.SetVector(_windDirectionId, _windDirection);
        }

        private void Update()
        {
            _windPosition += _windSpeed * Time.deltaTime;

            _material.SetVector(_windPositionId, _windPosition);
        }
    }
}