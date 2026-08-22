using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.Managers
{
    [Serializable]
    public class TownHallLevelInfo
    {
        public int level;
        public int woodCost;
        public int stoneCost;
        public int foodCost;
        public int maxRoadDistance; // np. maksymalny promień/zasięg sieci drogowej
        public string description;
    }

    public class TownHallManager : MonoBehaviour
    {
        public static TownHallManager Instance { get; private set; }

        public int CurrentLevel { get; private set; } = 1;
        public const int MAX_LEVEL = 7;

        [Header("Level Configs (7 Levels)")]
        [SerializeField] private List<TownHallLevelInfo> levels = new List<TownHallLevelInfo>();

        public static event Action<int> OnTownHallUpgraded;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeDefaultLevels();
        }

        private void InitializeDefaultLevels()
        {
            if (levels.Count > 0) return;

            // Domyślne koszty ulepszeń od poziomu 1 do 7
            for (int i = 1; i <= MAX_LEVEL; i++)
            {
                levels.Add(new TownHallLevelInfo
                {
                    level = i,
                    woodCost = i * 50,
                    stoneCost = i * 30,
                    foodCost = i * 20,
                    maxRoadDistance = 10 + (i * 5),
                    description = $"Ratusz Poziom {i}"
                });
            }
        }

        public TownHallLevelInfo GetCurrentLevelInfo()
        {
            return levels.Find(l => l.level == CurrentLevel);
        }

        public TownHallLevelInfo GetNextLevelInfo()
        {
            if (CurrentLevel >= MAX_LEVEL) return null;
            return levels.Find(l => l.level == CurrentLevel + 1);
        }

        public bool UpgradeTownHall()
        {
            if (CurrentLevel >= MAX_LEVEL)
            {
                Debug.LogWarning("[TownHall] Osiągnięto maksymalny 7. poziom!");
                return false;
            }

            TownHallLevelInfo next = GetNextLevelInfo();
            if (next == null) return false;

            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.HasEnough(ResourceType.Wood, next.woodCost) &&
                    ResourceManager.Instance.HasEnough(ResourceType.Stone, next.stoneCost) &&
                    ResourceManager.Instance.HasEnough(ResourceType.RawFood, next.foodCost))
                {
                    ResourceManager.Instance.SubtractResource(ResourceType.Wood, next.woodCost);
                    ResourceManager.Instance.SubtractResource(ResourceType.Stone, next.stoneCost);
                    ResourceManager.Instance.SubtractResource(ResourceType.RawFood, next.foodCost);

                    CurrentLevel++;
                    Debug.Log($"[TownHall] Ulepszono Ratusz na poziom {CurrentLevel}!");
                    OnTownHallUpgraded?.Invoke(CurrentLevel);
                    return true;
                }
            }

            Debug.LogWarning("[TownHall] Brak wystarczających zasobów do ulepszenia Ratusza!");
            return false;
        }
    }
}