using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        // Zdarzenia do podłączenia interfejsu UI
        public event Action<ResourceType, double> OnResourceChanged;
        public event Action<ResourceType, double> OnProductionRateChanged;

        private readonly Dictionary<ResourceType, double> _resources = new Dictionary<ResourceType, double>();
        private readonly Dictionary<ResourceType, double> _productionRates = new Dictionary<ResourceType, double>();
    
        // Mnożniki mocy kliknięcia dla każdego zasobu (Domyślnie 1.0)
        private readonly Dictionary<ResourceType, double> _clickPowers = new Dictionary<ResourceType, double>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDictionaries();
        }

        private void Start()
        {
            // Subskrybujemy pętlę czasu
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += HandleTick;
            }

            // Zasoby początkowe
            AddResource(ResourceType.Wood, 1000);
            AddResource(ResourceType.Stone, 10000);
            AddResource(ResourceType.RawFood, 10000);
            AddResource(ResourceType.Hides, 10000);

        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= HandleTick;
            }
        }

        private void InitializeDictionaries()
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _resources[type] = 0;
                _productionRates[type] = 0;
                _clickPowers[type] = 1.0; // Bazowa moc kliknięcia = +1 per klik
            }
        }

        private void HandleTick(float tickDuration)
        {
            // Przy każdym ticku dodajemy wyprodukowane surowce
            foreach (var rate in _productionRates)
            {
                if (rate.Value > 0)
                {
                    AddResource(rate.Key, rate.Value * tickDuration);
                }
            }
        }

        public void HarvestByClick(ResourceType type)
        {
            double amountToGather = GetClickPower(type);
            AddResource(type, amountToGather);
            // Tu możemy w przyszłości dodać efekt cząsteczkowy "+1" latający nad myszką!
        }

        public double GetClickPower(ResourceType type) => _clickPowers.TryGetValue(type, out var val) ? val : 1.0;

        public void UpgradeClickPower(ResourceType type, double multiplier)
        {
            _clickPowers[type] *= multiplier;
        }
        // --- POBIERANIE DANYCH ---
        public double GetAmount(ResourceType type) => _resources.TryGetValue(type, out var val) ? val : 0;
        public double GetProductionRate(ResourceType type) => _productionRates.TryGetValue(type, out var val) ? val : 0;

        // --- MODYFIKACJA ZASOBÓW ---
        public void AddResource(ResourceType type, double amount)
        {
            if (amount <= 0) return;
            _resources[type] += amount;
            OnResourceChanged?.Invoke(type, _resources[type]);
        }

        public bool HasEnough(ResourceType type, double amount) => GetAmount(type) >= amount;

        public bool SubtractResource(ResourceType type, double amount)
        {
            if (!HasEnough(type, amount)) return false;
            _resources[type] -= amount;
            OnResourceChanged?.Invoke(type, _resources[type]);
            return true;
        }

        // --- MODYFIKACJA PRODUKCJI ---
        public void SetProductionRate(ResourceType type, double ratePerSecond)
        {
            _productionRates[type] = Math.Max(0, ratePerSecond);
            OnProductionRateChanged?.Invoke(type, _productionRates[type]);
        }

        public void AddProductionRate(ResourceType type, double deltaRate)
        {
            SetProductionRate(type, GetProductionRate(type) + deltaRate);
        }
    }
}