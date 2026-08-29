using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;
using IdleBuilder.Managers;

namespace IdleBuilder.World
{

    public class RoadNetworkManager : MonoBehaviour
    {
        public static RoadNetworkManager Instance { get; private set; }



        // ============================================================
        // NOWY SYSTEM TIERÓW
        // ============================================================

        [Header("Konfiguracja Dróg")]
        [SerializeField] private List<RoadTierConfig> roadTierConfigs =
            new List<RoadTierConfig>();

        [SerializeField] private RoadTier currentSelectedTier =
            RoadTier.Prehistoric;

        public RoadTier CurrentSelectedTier => currentSelectedTier;

        // ============================================================
        // DANE DRÓG
        // ============================================================

        // Zastępuje stary HashSet, zachowując dodatkowo tier drogi.
        private readonly Dictionary<Vector2Int, RoadTier> _roadTiles =
            new Dictionary<Vector2Int, RoadTier>();

        // Ręcznie ustawione maski.
        private readonly Dictionary<Vector2Int, int> _manualMasks =
            new Dictionary<Vector2Int, int>();

        // Stare połączenie z Town Hall.
        private readonly HashSet<Vector2Int> _connectedToTownHall =
            new HashSet<Vector2Int>();

        // Najlepszy bottleneck tier ścieżki do Town Hall.
        private readonly Dictionary<Vector2Int, RoadTier> _connectedPathBottleneck =
            new Dictionary<Vector2Int, RoadTier>();


        // ============================================================
        // UNITY
        // ============================================================

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
                roadFolder = "Graphics/Road/Era1/",
                bridgeFolder = "Graphics/Bridge/Era1/",
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
                roadFolder = "Graphics/Road/Era2/",
                bridgeFolder = "Graphics/Bridge/Era2/",
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
                roadFolder = "Graphics/Road/Era3/",
                bridgeFolder = "Graphics/Bridge/Era3/",
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
                roadFolder = "Graphics/Road/Era4/",
                bridgeFolder = "Graphics/Bridge/Era4/",
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
                roadFolder = "Graphics/Road/Era5/",
                bridgeFolder = "Graphics/Bridge/Era5/",
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
                roadFolder = "Graphics/Road/Era6/",
                bridgeFolder = "Graphics/Bridge/Era6/",
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
                roadFolder = "Graphics/Road/Era7/",
                bridgeFolder = "Graphics/Bridge/Era7/",
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


        public void SetSelectedRoadTier(RoadTier newTier)
        {
            currentSelectedTier = newTier;

            Debug.Log(
                $"[RoadNetworkManager] Wybrano typ drogi: {newTier}"
            );
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

    

        public bool PlaceRoad(TileView tile)
        {
            

            return true;
        } 


        public bool RemoveRoad(TileView tile)
        {
            return true;
        }




        public int GetRoadConnectionMask(
            Vector2Int pos)
        {
            int mask = 0;

            if (HasRoadOrTownHall(
                    pos + Vector2Int.up))
            {
                mask |=
                    (int)RoadDirections.North;
            }

            if (HasRoadOrTownHall(
                    pos + Vector2Int.right))
            {
                mask |=
                    (int)RoadDirections.East;
            }

            if (HasRoadOrTownHall(
                    pos + Vector2Int.down))
            {
                mask |=
                    (int)RoadDirections.South;
            }

            if (HasRoadOrTownHall(
                    pos + Vector2Int.left))
            {
                mask |=
                    (int)RoadDirections.West;
            }

            return mask;
        }

        private bool HasRoadOrTownHall(
            Vector2Int pos)
        {
            if (GridManager.Instance == null)
                return false;

            TileView tile =
                GridManager.Instance.GetTileAt(pos);

            if (tile == null)
                return false;

            return _roadTiles.ContainsKey(pos) ||
                   tile.Type == TileType.TownHall;
        }


        // ============================================================
        // POŁĄCZENIA Z TOWN HALL
        // ============================================================

        public void RecalculateTownHallConnections()
        {
            _connectedToTownHall.Clear();
            _connectedPathBottleneck.Clear();

            if (GridManager.Instance == null)
                return;

            Queue<(Vector2Int pos, RoadTier bottleneck)>
                queue =
                    new Queue<(Vector2Int, RoadTier)>();

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (var kvp in _roadTiles)
            {
                Vector2Int roadPos = kvp.Key;
                RoadTier roadTier = kvp.Value;

                if (IsAdjacentToTownHall(roadPos))
                {
                    _connectedToTownHall.Add(
                        roadPos
                    );

                    _connectedPathBottleneck[
                        roadPos
                    ] = roadTier;

                    queue.Enqueue(
                        (roadPos, roadTier)
                    );
                }
            }

            while (queue.Count > 0)
            {
                var (
                    currentPos,
                    currentBottleneck
                ) = queue.Dequeue();

                foreach (var dir in directions)
                {
                    Vector2Int neighbor =
                        currentPos + dir;

                    if (!_roadTiles.TryGetValue(
                            neighbor,
                            out RoadTier neighborTier))
                    {
                        continue;
                    }

                    RoadTier newBottleneck =
                        neighborTier < currentBottleneck
                            ? neighborTier
                            : currentBottleneck;

                    bool wasConnected =
                        _connectedPathBottleneck.TryGetValue(
                            neighbor,
                            out RoadTier existingBottleneck);

                    // Jeśli znaleźliśmy lepszą ścieżkę,
                    // aktualizujemy bottleneck.
                    if (!wasConnected ||
                        newBottleneck > existingBottleneck)
                    {
                        _connectedToTownHall.Add(
                            neighbor
                        );

                        _connectedPathBottleneck[
                            neighbor
                        ] = newBottleneck;

                        queue.Enqueue(
                            (neighbor, newBottleneck)
                        );
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

        public bool IsRoadConnectedToTownHall(
            Vector2Int pos)
        {
            return _connectedToTownHall.Contains(pos);
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

        public RoadBonusResult GetBuildingRoadBonus(
            Vector2Int buildingPos)
        {
            RoadBonusResult result =
                new RoadBonusResult
                {
                    isConnectedToTownHall = false,
                    effectiveTier =
                        RoadTier.Prehistoric,
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

            RoadTier bestPathTier =
                RoadTier.Prehistoric;

            bool connected = false;

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
                    connected = true;
                    bestPathTier =
                        RoadTier.Fusion;

                    break;
                }

                if (_connectedPathBottleneck.TryGetValue(
                        neighbor,
                        out RoadTier pathBottleneck))
                {
                    connected = true;

                    if (pathBottleneck >
                        bestPathTier)
                    {
                        bestPathTier =
                            pathBottleneck;
                    }
                }
            }

            if (connected)
            {
                RoadTierConfig config =
                    GetConfig(bestPathTier);

                if (config != null)
                {
                    result.isConnectedToTownHall =
                        true;

                    result.effectiveTier =
                        bestPathTier;

                    result.productionBonusPercent =
                        config.productionBonusPercent;

                    result.maintenanceDiscountPercent =
                        config.maintenanceDiscountPercent;
                }
            }

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

            if (_roadTiles.TryGetValue(tile.GridPosition, out RoadTier existingTier))
            {
                currentTier = existingTier;
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

            List<ResourceCost> result =
                new List<ResourceCost>();

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

        public bool CanBuildOrUpgradeRoad(
            TileView tile,
            RoadTier targetTier,
            out List<ResourceCost> costs)
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
            if (_roadTiles.TryGetValue(
                    tile.GridPosition,
                    out RoadTier currentTier))
            {
                // Nie można "ulepszyć" do tego samego
                // lub niższego tieru.
                if (targetTier <= currentTier)
                    return false;
            }

            costs = GetTotalBuildCost(
                tile,
                targetTier
            );

            return costs.Count > 0;
        }

        public bool HasEnoughResourcesForRoad(
            List<ResourceCost> costs)
        {
            if (ResourceManager.Instance == null)
                return true;

            foreach (ResourceCost cost in costs)
            {
                if (!ResourceManager.Instance.HasEnough(
                        cost.type,
                        cost.amount))
                {
                    return false;
                }
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
    }
}