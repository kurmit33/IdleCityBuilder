using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;
using IdleBuilder.Managers;

using IdleBuilder.UI;

namespace IdleBuilder.World
{
    public class RoadPlacementManager : MonoBehaviour
    {
        public static RoadPlacementManager Instance { get; private set; }

        [Header("Ustawienia Trybu")]
        [SerializeField] private bool isBuildModeActive = false;

        public event Action<TileView> OnRoadBuilt;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }




        public void SetBuildMode(bool active)
        {
            isBuildModeActive = active;
        }



        public List<ResourceCost> CalculateCostForTile(TileView tile, RoadTier tier)
        {
            List<ResourceCost> result = new List<ResourceCost>();
            RoadTierConfig config = RoadNetworkManager.Instance.GetConfig(tier);

            float multiplier = tile.Type switch
            {
                TileType.Plains => 1.0f,
                TileType.Forest => 1.5f,
                TileType.Mountain => 2.5f,
                TileType.River => 3.0f,
                TileType.Ocean => 4.5f,
                _ => 1.0f
            };

            foreach (var cost in config.baseCosts)
            {
                result.Add(new ResourceCost
                {
                    type = cost.type,
                    amount = Mathf.CeilToInt(cost.amount * multiplier)
                });
            }

            return result;
        }

        public bool HasEnoughResources(List<ResourceCost> costs)
        {
            if (ResourceManager.Instance == null) return true;
            foreach (var cost in costs)
            {
                if (!ResourceManager.Instance.HasEnough(cost.type, cost.amount)) return false;
            }
            return true;
        }

        public bool TryBuildRoadOnTile(TileView tile)
        {
            RoadTier tier = RoadNetworkManager.Instance.CurrentSelectedTier;
            List<ResourceCost> costs = CalculateCostForTile(tile, tier);

            if (!HasEnoughResources(costs)) return false;
            if (!isBuildModeActive) return false;
            if (ResourceManager.Instance != null)
            {
                foreach (var cost in costs)
                {
                    ResourceManager.Instance.SubtractResource(cost.type, cost.amount);
                }
            }
            RoadUI.Instance.ApplyRoadToTile(tile);
            return true;            

        }
    }
}