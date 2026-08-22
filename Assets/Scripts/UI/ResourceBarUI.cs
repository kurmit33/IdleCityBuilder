using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;
using IdleBuilder.Managers;

namespace IdleBuilder.UI
{
    public class ResourceBarUI : MonoBehaviour
    {
        [Header("Prefabs & Containers")]
        [SerializeField] private ResourceItemUI itemPrefab;
        [SerializeField] private Transform container;

        [Header("Manual Icon Overrides (Optional)")]
        [SerializeField] private List<ResourceIconMapping> iconMappings;

        private readonly Dictionary<ResourceType, ResourceItemUI> _activeItems = new Dictionary<ResourceType, ResourceItemUI>();
        private readonly Dictionary<ResourceType, Sprite> _iconCache = new Dictionary<ResourceType, Sprite>();

        [System.Serializable]
        public struct ResourceIconMapping
        {
            public ResourceType type;
            public Sprite icon;
        }

        private void Awake()
        {
            foreach (var mapping in iconMappings)
            {
                if (!_iconCache.ContainsKey(mapping.type) && mapping.icon != null)
                    _iconCache.Add(mapping.type, mapping.icon);
            }
            PreloadAllIcons();
        }

        private void PreloadAllIcons()
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (!_iconCache.ContainsKey(type))
                {
                    Sprite loadedIcon = Resources.Load<Sprite>($"Graphics/Icons/{type}");
                    if (loadedIcon != null) _iconCache.Add(type, loadedIcon);
                }
            }
        }

        private void Start()
        {
            // Subskrypcja Ekonomii
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
                ResourceManager.Instance.OnProductionRateChanged += HandleProductionRateChanged;
            }

            // Subskrypcja Zmiany Ery
            if (EraManager.Instance != null)
            {
                EraManager.Instance.OnEraChanged += HandleEraChanged;
            }

            // Inicjalne załadowanie UI na podstawie obecnej ery!
            RefreshUnlockedResources();
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
                ResourceManager.Instance.OnProductionRateChanged -= HandleProductionRateChanged;
            }
            if (EraManager.Instance != null)
            {
                EraManager.Instance.OnEraChanged -= HandleEraChanged;
            }
        }

        private void HandleEraChanged(EraType newEra)
        {
            // Gdy zmieniamy erę, odświeżamy pasek, by dodać nowe ikonki (wyświetlą się początkowo z wartością 0)
            RefreshUnlockedResources();
        }

        private void RefreshUnlockedResources()
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                // Zapytaj EraManagera, czy surowiec jest odblokowany
                if (EraManager.Instance != null && EraManager.Instance.IsResourceUnlocked(type))
                {
                    ResourceItemUI item = GetOrCreateItem(type);
                    
                    // Wymuś zaktualizowanie tekstu (ustawi "0", dopóki gracz czegoś nie zbierze)
                    double currentAmount = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(type) : 0;
                    double currentRate = ResourceManager.Instance != null ? ResourceManager.Instance.GetProductionRate(type) : 0;
                    item.UpdateDisplay(currentAmount, currentRate);
                }
            }
        }

        private void HandleResourceChanged(ResourceType type, double newAmount)
        {
            // Aktualizuj UI TYLKO jeśli ten surowiec jest odblokowany i widoczny na pasku
            if (_activeItems.TryGetValue(type, out var item))
            {
                double rate = ResourceManager.Instance.GetProductionRate(type);
                item.UpdateDisplay(newAmount, rate);
            }
        }

        private void HandleProductionRateChanged(ResourceType type, double newRate)
        {
            if (_activeItems.TryGetValue(type, out var item))
            {
                double amount = ResourceManager.Instance.GetAmount(type);
                item.UpdateDisplay(amount, newRate);
            }
        }

        private ResourceItemUI GetOrCreateItem(ResourceType type)
        {
            if (_activeItems.TryGetValue(type, out var existingItem))
            {
                return existingItem;
            }

            ResourceItemUI newItem = Instantiate(itemPrefab, container);
            
            _iconCache.TryGetValue(type, out Sprite icon);
            newItem.Setup(type, icon);

            _activeItems.Add(type, newItem);
            return newItem;
        }
    }
}