using System;
using UnityEngine;

namespace Dummies
{
    public class Health : MonoBehaviour
    {
        public event Action<int> OnHealthChanged;
        public event Action<float> OnHealthChanged01;
        public event Action OnDeath;

        [SerializeField] protected CharacterController _characterController;
        [SerializeField] protected int _maxHealth;

        protected int _health;
        protected bool _isAlive;

        public int HealthValue
        {
            get => _health;
            set
            {
                if (_health == value)
                    return;
                _health = value;
                OnHealthChanged?.Invoke(_health);
                OnHealthChanged01?.Invoke(_health / (float)_maxHealth);
            }
        }

        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                if (_isAlive == value)
                    return;
                _isAlive = value;
                if (!_isAlive)
                    OnDeath?.Invoke();
            }
        }
        
        public float HealthValue01 => HealthValue / (float)_maxHealth;
        public int MaxHealthValue => _maxHealth;
        public Collider Collider => _characterController;

        protected virtual void Awake()
        {
            SetHealth(MaxHealthValue);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive)
                return;

            HealthValue = Mathf.Clamp(HealthValue - damage, 0, _maxHealth);
            if (HealthValue <= 0)
                IsAlive = false;
        }

        public void Heal(int heal)
        {
            if (!IsAlive)
                return;
            
            HealthValue = Mathf.Clamp(HealthValue + heal, 0, _maxHealth);
        }

        public void SetHealth(int health)
        {
            HealthValue = Mathf.Clamp(health, 0, _maxHealth);
            IsAlive = HealthValue > 0;
        }
    }
}