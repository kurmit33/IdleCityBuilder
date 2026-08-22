using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.Managers
{
    public class ExpansionManager : MonoBehaviour
    {
        public static ExpansionManager Instance { get; private set; }

        [Header("Ekspansja")]
        private int _totalExpansionsPurchased = 0;

        // Limity zakupu dla poszczególnych Er
        private readonly Dictionary<EraType, int> _maxExpansionsPerEra = new Dictionary<EraType, int>
        {
            { EraType.Prehistory, 8 },
            { EraType.Antiquity, 20 },
            { EraType.MiddleAges, 36 },
            { EraType.Industrial, 56 },
            { EraType.Atomic, 80 },
            { EraType.Digital, 108 },
            { EraType.Fusion, 140 }
        };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Sprawdza czy gracz zmieścił się w limicie ery
        public bool HasEraCapacity(EraType currentEra)
        {
            if (!_maxExpansionsPerEra.ContainsKey(currentEra)) return true;
            return _totalExpansionsPurchased < _maxExpansionsPerEra[currentEra];
        }

        public int GetRemainingExpansionsInEra(EraType currentEra)
        {
            if (!_maxExpansionsPerEra.ContainsKey(currentEra)) return 0;
            return Mathf.Max(0, _maxExpansionsPerEra[currentEra] - _totalExpansionsPurchased);
        }

        // Sprawdza, czy gracz ma wystarczająco WSZYSTKICH wymaganych zasobów
        public bool HasEnoughResourcesForExpansion(EraType currentEra)
        {
            if (ResourceManager.Instance == null) return false;

            var costs = GetExpansionCost(currentEra);
            foreach (var kvp in costs)
            {
                if (!ResourceManager.Instance.HasEnough(kvp.Key, kvp.Value))
                {
                    return false;
                }
            }
            return true;
        }

        // Całkowity warunek zakupu (limit ery + zasoby)
        public bool CanBuyExpansion(EraType currentEra)
        {
            return HasEraCapacity(currentEra) && HasEnoughResourcesForExpansion(currentEra);
        }

        // Dynamiczne koszty rozbudowy w zależności od Ery (od 2 do 8 zasobów)
        public Dictionary<ResourceType, double> GetExpansionCost(EraType currentEra)
        {
            var cost = new Dictionary<ResourceType, double>();
            double multiplier = Mathf.Pow(1.35f, _totalExpansionsPurchased);

            switch (currentEra)
            {
                case EraType.Prehistory:
                    cost[ResourceType.Wood] = System.Math.Round(40 * multiplier);
                    cost[ResourceType.Stone] = System.Math.Round(25 * multiplier);
                    break;

                case EraType.Antiquity:
                    cost[ResourceType.Wood] = System.Math.Round(120 * multiplier);
                    cost[ResourceType.Stone] = System.Math.Round(80 * multiplier);
                    cost[ResourceType.RawFood] = System.Math.Round(60 * multiplier);
                    cost[ResourceType.Gold] = System.Math.Round(15 * multiplier);
                    break;

                case EraType.MiddleAges:
                    cost[ResourceType.Wood] = System.Math.Round(300 * multiplier);
                    cost[ResourceType.Stone] = System.Math.Round(250 * multiplier);
                    cost[ResourceType.RawFood] = System.Math.Round(150 * multiplier);
                    cost[ResourceType.Clay] = System.Math.Round(80 * multiplier);
                    cost[ResourceType.IronOre] = System.Math.Round(50 * multiplier);
                    break;

                case EraType.Industrial:
                    cost[ResourceType.Wood] = System.Math.Round(500 * multiplier);
                    cost[ResourceType.Stone] = System.Math.Round(500 * multiplier);
                    cost[ResourceType.IronOre] = System.Math.Round(300 * multiplier);
                    cost[ResourceType.Coal] = System.Math.Round(200 * multiplier);
                    cost[ResourceType.Steel] = System.Math.Round(100 * multiplier);
                    cost[ResourceType.Gold] = System.Math.Round(150 * multiplier);
                    break;

                // Domyślna obsługa zaawansowanych er z wieloma zasobami (do 8 zasobów)
                default:
                    cost[ResourceType.Wood] = System.Math.Round(1000 * multiplier);
                    cost[ResourceType.Stone] = System.Math.Round(1000 * multiplier);
                    cost[ResourceType.IronOre] = System.Math.Round(800 * multiplier);
                    cost[ResourceType.Coal] = System.Math.Round(600 * multiplier);
                    cost[ResourceType.Steel] = System.Math.Round(500 * multiplier);
                    cost[ResourceType.Gold] = System.Math.Round(400 * multiplier);
                    cost[ResourceType.Uranium] = System.Math.Round(300 * multiplier);
                    cost[ResourceType.Clay] = System.Math.Round(250 * multiplier);
                    break;
            }

            return cost;
        }

        public void RegisterExpansionPurchase()
        {
            _totalExpansionsPurchased++;
        }

        // Metoda dokonująca zakupu - pobiera zasoby z ResourceManager i zwiększa licznik
        public bool TryBuyExpansion(EraType currentEra)
        {
            if (!CanBuyExpansion(currentEra)) return false;

            var costs = GetExpansionCost(currentEra);

            // Pobieramy surowce z portfela gracza
            foreach (var kvp in costs)
            {
                ResourceManager.Instance.SubtractResource(kvp.Key, kvp.Value);
            }

            RegisterExpansionPurchase();
            return true;
        }
    }
}