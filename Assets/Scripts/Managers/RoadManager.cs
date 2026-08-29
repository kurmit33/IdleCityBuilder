using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.UI;
using IdleBuilder.Core;
using IdleBuilder.Managers;

namespace IdleBuilder.World
{

    public class RoadManager : MonoBehaviour
    {
        public static RoadManager Instance { get; private set; }



        // ============================================================
        // NOWY SYSTEM TIERÓW
        // ============================================================

        [Header("Konfiguracja Dróg")]
        [SerializeField] private List<RoadTierConfig> roadTierConfigs = new List<RoadTierConfig>();


        // Stare połączenie z Town Hall.
        private readonly HashSet<Vector2Int> _connectedToTownHall = new HashSet<Vector2Int>();

        // Najlepszy bottleneck tier ścieżki do Town Hall.
        private readonly Dictionary<Vector2Int, RoadTier> _connectedPathBottleneck = new Dictionary<Vector2Int, RoadTier>();
        private readonly Dictionary<Vector2Int, int> _connectedPathDistance = new Dictionary<Vector2Int, int>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }


            // Nowy system.
            InitDefaultTierConfigs();
        }


        public IReadOnlyList<RoadTierConfig> GetAllConfigs()
        {
            return roadTierConfigs;
        }

        // ============================================================
        // KONFIGURACJA TIERÓW
        // ============================================================

        private void InitDefaultTierConfigs()
        {
            if (roadTierConfigs.Count > 0)
                return;

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.Prehistoric,
                tierName = "Ścieżka Ubita",
                eraRequirement = EraType.Prehistory,
                productionBonusPercent = -5f,
                maintenanceDiscountPercent = -5f,
                roadFolder = "Graphics/Road/Era_1/",
                bridgeFolder = "Graphics/Bridge/Era_1/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Wood,
                        amount = 10
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Stone,
                        amount = 8
                    }
                }
            });

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.Antiquity,
                tierName = "Trakt Kamienno-Ilasty",
                eraRequirement = EraType.Antiquity,
                productionBonusPercent = 5f,
                maintenanceDiscountPercent = 0f,
                roadFolder = "Graphics/Road/Era_2/",
                bridgeFolder = "Graphics/Bridge/Era_2/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Stone,
                        amount = 20
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Clay,
                        amount = 10
                    },
                    new ResourceCost
                    {
                        type = ResourceType.CopperOre,
                        amount = 2
                    }
                }
            });

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.MiddleAges,
                tierName = "Brukowany Gościniec",
                eraRequirement = EraType.MiddleAges,
                productionBonusPercent = 20f,
                maintenanceDiscountPercent = 10f,
                roadFolder = "Graphics/Road/Era_3/",
                bridgeFolder = "Graphics/Bridge/Era_3/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Lumber,
                        amount = 5
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Limestone,
                        amount = 4
                    },
                    new ResourceCost
                    {
                        type = ResourceType.IronOre,
                        amount = 2
                    }
                }
            });

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.Industrial,
                tierName = "Szosa Asfaltowa",
                eraRequirement = EraType.Industrial,
                productionBonusPercent = 35f,
                maintenanceDiscountPercent = 18f,
                roadFolder = "Graphics/Road/Era_4/",
                bridgeFolder = "Graphics/Bridge/Era_4/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Steel,
                        amount = 4
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Coal,
                        amount = 6
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Rubber,
                        amount = 2
                    }
                }
            });

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.Atomic,
                tierName = "Autostrada Betonowa",
                eraRequirement = EraType.Atomic,
                productionBonusPercent = 50f,
                maintenanceDiscountPercent = 25f,
                roadFolder = "Graphics/Road/Era_5/",
                bridgeFolder = "Graphics/Bridge/Era_5/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Steel,
                        amount = 6
                    },
                    new ResourceCost
                    {
                        type = ResourceType.RefinedFuel,
                        amount = 4
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Plastics,
                        amount = 3
                    }
                }
            });

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.Digital,
                tierName = "Tor Maglev",
                eraRequirement = EraType.Digital,
                productionBonusPercent = 70f,
                maintenanceDiscountPercent = 35f,
                roadFolder = "Graphics/Road/Era_6/",
                bridgeFolder = "Graphics/Bridge/Era_6/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Silicon,
                        amount = 5
                    },
                    new ResourceCost
                    {
                        type = ResourceType.AdvancedPolymers,
                        amount = 4
                    },
                    new ResourceCost
                    {
                        type = ResourceType.LiIonBatteries,
                        amount = 2
                    }
                }
            });

            roadTierConfigs.Add(new RoadTierConfig
            {
                tier = RoadTier.Fusion,
                tierName = "Tuba Hyperloop",
                eraRequirement = EraType.Fusion,
                productionBonusPercent = 100f,
                maintenanceDiscountPercent = 50f,
                roadFolder = "Graphics/Road/Era_7/",
                bridgeFolder = "Graphics/Bridge/Era_7/",
                baseCosts = new List<ResourceCost>
                {
                    new ResourceCost
                    {
                        type = ResourceType.Graphene,
                        amount = 4
                    },
                    new ResourceCost
                    {
                        type = ResourceType.TitaniumAlloys,
                        amount = 3
                    },
                    new ResourceCost
                    {
                        type = ResourceType.Superconductors,
                        amount = 2
                    }
                }
            });
        }

        public RoadTierConfig GetConfig(RoadTier tier)
        {
            RoadTierConfig config =
                roadTierConfigs.Find(
                    c => c.tier == tier
                );

            if (config != null)
                return config;

            if (roadTierConfigs.Count > 0)
                return roadTierConfigs[0];

            Debug.LogError(
                "[RoadNetworkManager] Brak konfiguracji tierów dróg!"
            );

            return null;
        }


        public bool RemoveRoad(TileView tile)
        {
            Vector2Int position = tile.GridPosition;

            tile.SetHasRoad(false);
            tile.SetRoadTier(RoadTier.None);

            // Zbudowany kafelek nie ma już drogi,
            // więc nie dostanie sprite'a.
            // Sąsiedzi muszą natomiast przeliczyć maskę.
            RefreshRoadMasksAround(position);

            RecalculateTownHallConnections();

            return true;
        }




        public int GetRoadConnectionMask(Vector2Int pos, Vector2Int? previewPosition = null)
        {
            int mask = 0;

            if (HasRoadOrTownHall(pos + Vector2Int.up, previewPosition))
            {
                mask |= (int)RoadDirections.North;
            }

            if (HasRoadOrTownHall(pos + Vector2Int.right, previewPosition))
            {
                mask |= (int)RoadDirections.East;
            }

            if (HasRoadOrTownHall( pos + Vector2Int.down, previewPosition))
            {
                mask |= (int)RoadDirections.South;
            }

            if (HasRoadOrTownHall(pos + Vector2Int.left, previewPosition))
            {
                mask |= (int)RoadDirections.West;
            }

            return mask;
        }

        private bool HasRoadOrTownHall(Vector2Int pos, Vector2Int? previewPosition = null)
        {
            if (GridManager.Instance == null) return false;

            TileView tile = GridManager.Instance.GetTileAt(pos);

            if (tile == null) return false;
            if (previewPosition.HasValue && pos == previewPosition.Value) return true;

            return tile.HasRoad || tile.Type == TileType.TownHall;
        }

        private void RefreshRoadMasksAround(Vector2Int position)
        {
            UpdateRoadMask(position);

            UpdateRoadMask(position + Vector2Int.up);
            UpdateRoadMask(position + Vector2Int.right);
            UpdateRoadMask(position + Vector2Int.down);
            UpdateRoadMask(position + Vector2Int.left);
        }

        private void UpdateRoadMask(Vector2Int position)
        {
            TileView tile = GridManager.Instance.GetTileAt(position);

            if (tile == null || !tile.HasRoad)
                return;

            int mask = GetRoadConnectionMask(position);

            RoadUI.Instance?.ApplyRoadMask(
                tile,
                mask
            );
        }

        public bool PlaceRoad(TileView tile)
        {
            if (tile == null)
                return false;

            if (!CanBuildOrUpgradeRoad(
                    tile,
                    RoadUI.Instance.SelectedRoadTier,
                    out List<ResourceCost> costs))
            {
                return false;
            }

            if (!HasEnoughResourcesForRoad(costs))
                return false;

            PayRoadCosts(costs);

            tile.SetRoadTier(RoadUI.Instance.SelectedRoadTier);
            RoadUI.Instance?.HideAll();
            tile.SetHasRoad(true);

            RefreshRoadMasksAround(tile.GridPosition);

            RecalculateTownHallConnections();

            return true;
        }


        public void RecalculateTownHallConnections()
        {
            _connectedToTownHall.Clear();
            _connectedPathBottleneck.Clear();
            _connectedPathDistance.Clear();

            if (GridManager.Instance == null)
                return;

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            Queue<Vector2Int> queue =
                new Queue<Vector2Int>();

            foreach (TileView tile in GridManager.Instance.GetAllTiles())
            {
                if (tile == null || !tile.HasRoad)
                    continue;

                if (!IsAdjacentToTownHall(tile.GridPosition))
                    continue;

                Vector2Int position = tile.GridPosition;

                _connectedToTownHall.Add(position);
                _connectedPathDistance[position] = 1;
                _connectedPathBottleneck[position] = tile.RoadTier;

                queue.Enqueue(position);
            }

            while (queue.Count > 0)
            {
                Vector2Int currentPosition = queue.Dequeue();

                int currentDistance =
                    _connectedPathDistance[currentPosition];

                RoadTier currentBottleneck =
                    _connectedPathBottleneck[currentPosition];

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int neighborPosition =
                        currentPosition + direction;

                    TileView neighborTile =
                        GridManager.Instance.GetTileAt(
                            neighborPosition
                        );

                    if (neighborTile == null ||
                        !neighborTile.HasRoad)
                    {
                        continue;
                    }

                    int newDistance =
                        currentDistance + 1;

                    RoadTier newBottleneck =
                        neighborTile.RoadTier < currentBottleneck
                            ? neighborTile.RoadTier
                            : currentBottleneck;

                    bool alreadyConnected =
                        _connectedPathDistance.TryGetValue(
                            neighborPosition,
                            out int existingDistance
                        );

                    // Pierwsza znaleziona ścieżka jest najkrótsza.
                    if (!alreadyConnected)
                    {
                        _connectedToTownHall.Add(neighborPosition);

                        _connectedPathDistance[
                            neighborPosition
                        ] = newDistance;

                        _connectedPathBottleneck[
                            neighborPosition
                        ] = newBottleneck;

                        queue.Enqueue(neighborPosition);

                        continue;
                    }

                    // Dłuższa trasa nas nie interesuje.
                    if (newDistance > existingDistance)
                        continue;

                    RoadTier existingBottleneck =
                        _connectedPathBottleneck[
                            neighborPosition
                        ];

                    // Ta sama długość → lepszy najgorszy tier.
                    if (newDistance == existingDistance &&
                        newBottleneck > existingBottleneck)
                    {
                        _connectedPathBottleneck[
                            neighborPosition
                        ] = newBottleneck;

                        queue.Enqueue(neighborPosition);
                    }
                }
            }
        }

        private bool IsAdjacentToTownHall(
            Vector2Int pos)
        {
            if (GridManager.Instance == null)
                return false;

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (var dir in directions)
            {
                TileView tile =
                    GridManager.Instance.GetTileAt(
                        pos + dir
                    );

                if (tile != null &&
                    tile.Type == TileType.TownHall)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBuildingConnectedToTownHall(
            Vector2Int buildingPos)
        {
            if (GridManager.Instance == null)
                return false;

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (var dir in directions)
            {
                Vector2Int neighbor =
                    buildingPos + dir;

                TileView neighborTile =
                    GridManager.Instance.GetTileAt(
                        neighbor
                    );

                if (neighborTile != null &&
                    neighborTile.Type ==
                    TileType.TownHall)
                {
                    return true;
                }

                if (_connectedToTownHall.Contains(
                        neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================================
        // BONUS DROGI DLA BUDYNKU
        // ============================================================

        public RoadBonusResult GetBuildingRoadBonus(Vector2Int buildingPos)
        {
            RoadBonusResult result =
                new RoadBonusResult
                {
                    isConnectedToTownHall = false,
                    effectiveTier = RoadTier.Prehistoric,
                    productionBonusPercent = 0f,
                    maintenanceDiscountPercent = 0f
                };

            if (GridManager.Instance == null)
                return result;

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            bool foundPath = false;

            int shortestDistance = int.MaxValue;

            RoadTier bestTier =
                RoadTier.Prehistoric;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int roadPosition =
                    buildingPos + direction;

                TileView roadTile =
                    GridManager.Instance.GetTileAt(
                        roadPosition
                    );

                // Budynek bezpośrednio przy Town Hall.
                if (roadTile != null &&
                    roadTile.Type == TileType.TownHall)
                {
                    foundPath = true;
                    shortestDistance = 0;
                    bestTier = RoadTier.Fusion;
                    break;
                }

                if (!_connectedPathDistance.TryGetValue(
                        roadPosition,
                        out int distance))
                {
                    continue;
                }

                if (!_connectedPathBottleneck.TryGetValue(
                        roadPosition,
                        out RoadTier pathTier))
                {
                    continue;
                }

                // Zawsze wybieramy najkrótszą trasę.
                if (distance < shortestDistance)
                {
                    foundPath = true;
                    shortestDistance = distance;
                    bestTier = pathTier;
                }
                // Jeżeli trasy są tak samo krótkie,
                // wybieramy lepszy bottleneck.
                else if (distance == shortestDistance &&
                        pathTier > bestTier)
                {
                    bestTier = pathTier;
                }
            }

            if (!foundPath)
                return result;

            RoadTierConfig config =
                GetConfig(bestTier);

            if (config == null)
                return result;

            result.isConnectedToTownHall = true;
            result.effectiveTier = bestTier;
            result.productionBonusPercent =
                config.productionBonusPercent;
            result.maintenanceDiscountPercent =
                config.maintenanceDiscountPercent;

            return result;
        }

        // ============================================================
        // KOSZTY BUDOWY I ULEPSZANIA DRÓG
        // ============================================================

        public List<ResourceCost> GetTotalBuildCost(TileView tile, RoadTier targetTier)
        {
            List<ResourceCost> result = new List<ResourceCost>();

            if (tile == null)
                return result;

            // Jeśli na kafelku już jest droga, liczymy koszt ulepszenia.
            RoadTier? currentTier = null;

            if (tile.HasRoad)
            {
                currentTier = tile.RoadTier;
            }   

            return CalculateTierCost(tile, currentTier, targetTier);
        }

        private List<ResourceCost> CalculateTierCost(
            TileView tile,
            RoadTier? currentTier,
            RoadTier targetTier)
        {
            Dictionary<ResourceType, int> combinedCosts =
                new Dictionary<ResourceType, int>();

            int firstTier = (int)RoadTier.Prehistoric;
            int targetTierValue = (int)targetTier;

            // Jeśli ulepszamy istniejącą drogę,
            // zaczynamy od tieru wyższego niż obecny.
            if (currentTier.HasValue)
            {
                firstTier = (int)currentTier.Value + 1;
            }

            // Budowa nowej drogi od razu na wyższym tierze:
            // płacimy za wszystkie wcześniejsze tiery.
            for (int tierValue = firstTier;
                tierValue <= targetTierValue;
                tierValue++)
            {
                RoadTier tier = (RoadTier)tierValue;

                RoadTierConfig config = GetConfig(tier);

                if (config == null || config.baseCosts == null)
                    continue;

                foreach (ResourceCost cost in config.baseCosts)
                {
                    if (combinedCosts.ContainsKey(cost.type))
                    {
                        combinedCosts[cost.type] += cost.amount;
                    }
                    else
                    {
                        combinedCosts[cost.type] = cost.amount;
                    }
                }
            }

            List<ResourceCost> result = new List<ResourceCost>();

            foreach (var pair in combinedCosts)
            {
                result.Add(new ResourceCost
                {
                    type = pair.Key,
                    amount = pair.Value
                });
            }

            return result;
        }

        public bool CanBuildOrUpgradeRoad(TileView tile, RoadTier targetTier, out List<ResourceCost> costs)
        {
            costs = new List<ResourceCost>();

            if (tile == null)
                return false;

            if (!tile.IsUnlocked)
                return false;

            if (tile.Type == TileType.TownHall)
                return false;

            // Ocean nadal blokujemy.
            if (tile.Type == TileType.Ocean)
                return false;

            // Jeżeli droga już istnieje.
            if (tile.HasRoad)
            {
                // Nie można "ulepszyć" do tego samego
                // lub niższego tieru.
                if (targetTier <= tile.RoadTier)
                    return false;
            }

            costs = GetTotalBuildCost(
                tile,
                targetTier
            );

            return costs.Count > 0;
        }

        public bool HasEnoughResourcesForRoad(List<ResourceCost> costs)
        {
            if (ResourceManager.Instance == null) return true;

            foreach (ResourceCost cost in costs)
            {
                if (!ResourceManager.Instance.HasEnough(cost.type, cost.amount)) return false;
            }

            return true;
        }

        private void PayRoadCosts(
            List<ResourceCost> costs)
        {
            if (ResourceManager.Instance == null)
                return;

            foreach (ResourceCost cost in costs)
            {
                ResourceManager.Instance.SubtractResource(
                    cost.type,
                    cost.amount
                );
            }
        }

        public List<ResourceCost> CalculateCostForTile(TileView tile, RoadTier tier)
        {
            List<ResourceCost> result = new List<ResourceCost>();
            RoadTierConfig config = GetConfig(tier);

            float multiplier = tile.Type switch
            {
                TileType.Plains => 1.0f,
                TileType.Forest => 2.5f,
                TileType.Mountain => 5.0f,
                TileType.River => 10.0f,
                TileType.Ocean => 30.0f,
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

    }
}