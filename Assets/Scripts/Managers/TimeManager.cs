using System;
using UnityEngine;

namespace IdleBuilder.Managers
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        // Zdarzenie wywoływane co każdy "Tick" (przekazuje czas trwania ticka w sekundach)
        public event Action<float> OnTick;

        [Header("Settings")]
        [Tooltip("Długość jednego ticka w sekundach")]
        [SerializeField] private float tickInterval = 1.0f;

        private float _timer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // Obsługa ticka — sprawnie radzi sobie też z ewentualnymi spadkami płynności
            while (_timer >= tickInterval)
            {
                _timer -= tickInterval;
                OnTick?.Invoke(tickInterval);
            }
        }
    }
}