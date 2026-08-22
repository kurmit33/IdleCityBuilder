using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.Managers
{
    public class EraManager : MonoBehaviour
    {
        public static EraManager Instance { get; private set; }

        // Zdarzenie wywoływane, gdy gracz awansuje do nowej ery
        public event Action<EraType> OnEraChanged;

        [Header("Current Era")]
        [SerializeField] private EraType currentEra = EraType.Prehistory;

        // Słownik mapujący dany zasób do ery, w której się pojawia
        private readonly Dictionary<ResourceType, EraType> _resourceEraMapping = new Dictionary<ResourceType, EraType>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeResourceEras();
        }

        private void InitializeResourceEras()
        {
            // --- ERA 1: PREHISTORIA ---
            _resourceEraMapping[ResourceType.Wood] = EraType.Prehistory;
            _resourceEraMapping[ResourceType.Stone] = EraType.Prehistory;
            _resourceEraMapping[ResourceType.RawFood] = EraType.Prehistory;
            _resourceEraMapping[ResourceType.Hides] = EraType.Prehistory;

            // --- ERA 2: STAROŻYTNOŚĆ ---
            _resourceEraMapping[ResourceType.CopperOre] = EraType.Antiquity;
            _resourceEraMapping[ResourceType.TinOre] = EraType.Antiquity;
            _resourceEraMapping[ResourceType.IronOre] = EraType.Antiquity;
            _resourceEraMapping[ResourceType.Grains] = EraType.Antiquity;
            _resourceEraMapping[ResourceType.Clay] = EraType.Antiquity;

            // --- ERA 3: ŚREDNIOWIECZE ---
            _resourceEraMapping[ResourceType.Lumber] = EraType.MiddleAges;
            _resourceEraMapping[ResourceType.Charcoal] = EraType.MiddleAges;
            _resourceEraMapping[ResourceType.Gold] = EraType.MiddleAges;
            _resourceEraMapping[ResourceType.Textiles] = EraType.MiddleAges;
            _resourceEraMapping[ResourceType.Limestone] = EraType.MiddleAges;

            // --- ERA 4: PRZEMYSŁ ---
            _resourceEraMapping[ResourceType.Coal] = EraType.Industrial;
            _resourceEraMapping[ResourceType.Steel] = EraType.Industrial;
            _resourceEraMapping[ResourceType.CrudeOil] = EraType.Industrial;
            _resourceEraMapping[ResourceType.Rubber] = EraType.Industrial;
            _resourceEraMapping[ResourceType.Cotton] = EraType.Industrial;

            // --- ERA 5: ATOM I WSPÓŁCZESNOŚĆ ---
            _resourceEraMapping[ResourceType.Uranium] = EraType.Atomic;
            _resourceEraMapping[ResourceType.RefinedCopper] = EraType.Atomic;
            _resourceEraMapping[ResourceType.RefinedFuel] = EraType.Atomic;
            _resourceEraMapping[ResourceType.Plastics] = EraType.Atomic;
            _resourceEraMapping[ResourceType.Lithium] = EraType.Atomic;

            // --- ERA 6: CYFROWA ---
            _resourceEraMapping[ResourceType.Silicon] = EraType.Digital;
            _resourceEraMapping[ResourceType.RareEarthMetals] = EraType.Digital;
            _resourceEraMapping[ResourceType.LiIonBatteries] = EraType.Digital;
            _resourceEraMapping[ResourceType.Data] = EraType.Digital;
            _resourceEraMapping[ResourceType.AdvancedPolymers] = EraType.Digital;

            // --- ERA 7: FUZJA I PRZYSZŁOŚĆ ---
            _resourceEraMapping[ResourceType.Helium3] = EraType.Fusion;
            _resourceEraMapping[ResourceType.Deuterium] = EraType.Fusion;
            _resourceEraMapping[ResourceType.Graphene] = EraType.Fusion;
            _resourceEraMapping[ResourceType.CarbonNanotubes] = EraType.Fusion;
            _resourceEraMapping[ResourceType.TitaniumAlloys] = EraType.Fusion;
            _resourceEraMapping[ResourceType.Metamaterials] = EraType.Fusion;
            _resourceEraMapping[ResourceType.Superconductors] = EraType.Fusion;
            _resourceEraMapping[ResourceType.QuantumCompute] = EraType.Fusion;
            _resourceEraMapping[ResourceType.SyntheticBiomass] = EraType.Fusion;
            _resourceEraMapping[ResourceType.Antimatter] = EraType.Fusion;
        }

        // Metoda sprawdzająca, czy zasób powinien być już widoczny
        public bool IsResourceUnlocked(ResourceType type)
        {
            if (_resourceEraMapping.TryGetValue(type, out EraType requiredEra))
            {
                // Porównujemy po wartości numerycznej enuma (np. czy 1 >= 1)
                return (int)currentEra >= (int)requiredEra;
            }
            return false;
        }

        // Metoda do wywoływania, gdy kupimy ulepszenie "Przejście do nowej ery"
        public void AdvanceEra()
        {
            if ((int)currentEra < 7)
            {
                currentEra++;
                OnEraChanged?.Invoke(currentEra);
                Debug.Log($"Awansowano do ery: {currentEra}");
            }
        }
    }
}