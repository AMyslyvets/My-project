using System;
using UnityEngine;

namespace Fiz
{
    public class HealthIndicator : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer _healthValue;
        [SerializeField] private SpriteRenderer _damageValue;
        [SerializeField] private Vector2 _referenceSize;
        [SerializeField] private float _damageDecaySpeed;
        [SerializeField] private float _regenerationSpeed;
        [SerializeField] private BillboardBase _billboard;
        
        private float _targetHealth;
        private float _displayedHealth;
        private float _displayedDamage;
        
        public BillboardBase Billboard => _billboard;
        
        private void Start()
        {
            ForceHealth(_health.HealthValue01);
            _health.OnHealthChanged01 += HealthChangedHandler;
        }

        private void Update()
        {
            if (_targetHealth < _displayedHealth)
                _displayedHealth = _targetHealth;
            else
            {
                _displayedHealth =
                    Mathf.MoveTowards(_displayedHealth, _targetHealth,
                    _regenerationSpeed * Time.deltaTime);
            }

            if (_displayedDamage > _displayedHealth)
            {
                _displayedDamage =
                    Mathf.MoveTowards(_displayedDamage, _displayedHealth,
                        _damageDecaySpeed * Time.deltaTime);
            }
            else
                _displayedDamage = _displayedHealth;
            
            _healthValue.size = new Vector2(_referenceSize.x * _displayedHealth, _referenceSize.y);
            _damageValue.size = new Vector2(_referenceSize.x * _displayedDamage, _referenceSize.y);
        }

        private void HealthChangedHandler(float health) => SetHealth(health);

        private void SetHealth(float health)
        {
            _targetHealth = health;
        }

        private void ForceHealth(float health)
        {
            _displayedDamage = _displayedHealth = _targetHealth = health;
        }
    }
}