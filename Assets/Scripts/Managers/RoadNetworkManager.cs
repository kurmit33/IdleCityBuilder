using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;
using IdleBuilder.Managers;

namespace IdleBuilder.World
{

    [Serializable]
    public class RoadSpriteEntry
    {
        public int maskValue; // Wartość 0 - 15 (bitmaska)
        public Sprite sprite;
    }

    public class RoadNetworkManager : MonoBehaviour
    {
        public static RoadNetworkManager Instance { get; private set; }

        [Header("Road Sprites Config (16 Variants)")]
        [Tooltip("Jeśli puste, ikony zostaną automatycznie pobrane z Resources/Graphics/Road/road_0 .. road_15")]
        [SerializeField] private List<RoadSpriteEntry> roadSprites = new List<RoadSpriteEntry>();
        [SerializeField] private List<RoadSpriteEntry> bridgeSprites = new List<RoadSpriteEntry>();

        // Przechowujemy pozycje kafelków z wybudowanymi drogami
        private readonly HashSet<Vector2Int> _roadTiles = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> _connectedToTownHall = new HashSet<Vector2Int>();
        private readonly Dictionary<int, Sprite> _roadSpriteLookup = new Dictionary<int, Sprite>();
        private readonly Dictionary<int, Sprite> _bridgeSpriteLookup = new Dictionary<int, Sprite>();

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

            BuildLookupTable();
        }

        private void BuildLookupTable()
        {
            _roadSpriteLookup.Clear();
            _bridgeSpriteLookup.Clear();

            int loadedFromResourcesCountRoad = 0;
            int loadedFromResourcesCountBridge = 0;

            // 1. Automatyczne ładowanie z folderu Resources/Graphics/Road/ (road_0 do road_15)
            for (int i = 0; i <= 15; i++)
            {
                Sprite loadedSprite = Resources.Load<Sprite>($"Graphics/Road/road_{i}");
                if (loadedSprite != null)
                {
                    _roadSpriteLookup[i] = loadedSprite;
                    loadedFromResourcesCountRoad++;
                }

                Sprite bridgeSprite = Resources.Load<Sprite>($"Graphics/Bridge/bridge_{i}");
                if (bridgeSprite != null) 
                {
                    _bridgeSpriteLookup[i] = bridgeSprite;
                    loadedFromResourcesCountBridge++;
                }
            }

            // 2. Jeśli brakuje części grafik z Resources, pobierz z listy z Inspektora
            foreach (var entry in roadSprites)
            {
                if (entry.sprite != null && !_roadSpriteLookup.ContainsKey(entry.maskValue))
                {
                    _roadSpriteLookup.Add(entry.maskValue, entry.sprite);
                }
            }
            
            foreach (var entry in bridgeSprites)
            {
                if (entry.sprite != null && !_bridgeSpriteLookup.ContainsKey(entry.maskValue))
                {
                    _bridgeSpriteLookup.Add(entry.maskValue, entry.sprite);
                }
            }

            Debug.Log($"[RoadNetworkManager] Załadowano {loadedFromResourcesCountRoad}/16 sprajtów dróg z Resources/Graphics/Road/.");
            Debug.Log($"[RoadNetworkManager] Załadowano {loadedFromResourcesCountBridge}/16 sprajtów mostów z Resources/Graphics/Bridge/.");
        }

        // --- 1. SPRAWDZANIE KOSZTÓW I WARUNKÓW BUDOWY ---
        public bool CanBuildRoadAt(TileView tile, out int woodCost, out int stoneCost)
        {
            woodCost = 0;
            stoneCost = 0;

            if (tile == null || !tile.IsUnlocked || tile.HasRoad) return false;

            // Nie można budować na Ratuszu ani Oceanie
            if (tile.Type == TileType.TownHall || tile.Type == TileType.Ocean)
                return false;


            // Koszty w zależności od biomu:
            switch (tile.Type)
            {
                case TileType.Plains:
                    woodCost = 2; stoneCost = 1;
                    break;
                case TileType.Forest:
                    woodCost = 5; stoneCost = 10; // Wycinka drzew
                    break;
                case TileType.Mountain:
                    woodCost = 5; stoneCost = 15; // Tunel / wyrównanie terenu
                    break;
                case TileType.River:
                    woodCost = 10; stoneCost = 10; // Budowa mostu
                    break;
                default:
                    return false;
            }

            return true;
        }

        // --- 2. BUDOWA I USUWANIE DRÓG ---
        public bool PlaceRoad(TileView tile)
        {
            if (!CanBuildRoadAt(tile, out int woodCost, out int stoneCost))
                return false;

            // Pobieranie zasobów
            if (ResourceManager.Instance != null)
            {
                if (!ResourceManager.Instance.HasEnough(ResourceType.Wood, woodCost) ||
                    !ResourceManager.Instance.HasEnough(ResourceType.Stone, stoneCost))
                {
                    Debug.LogWarning("[RoadManager] Za mało zasobów na budowę drogi!");
                    return false;
                }

                ResourceManager.Instance.SubtractResource(ResourceType.Wood, woodCost);
                ResourceManager.Instance.SubtractResource(ResourceType.Stone, stoneCost);
            }

            // Oznaczenie kafelka jako posiadającego drogę
            _roadTiles.Add(tile.GridPosition);
            tile.SetHasRoad(true);

            // Aktualizacja grafiki drogi dla siebie i sąsiadów
            UpdateRoadVisualsAround(tile.GridPosition);

            // Przeliczenie sieci połączeń z Ratuszem
            RecalculateTownHallConnections();

            Debug.Log($"[RoadManager] Zbudowano drogę na pozycji {tile.GridPosition}!");
            return true;
        }

        /// <summary>
        /// Rejestruje drogę bez pobierania surowców (np. przy wczytywaniu zapisu gry).
        /// </summary>
        public void RegisterRoad(TileView tile)
        {
            if (tile == null || _roadTiles.Contains(tile.GridPosition)) return;

            _roadTiles.Add(tile.GridPosition);
            tile.SetHasRoad(true);
            UpdateRoadVisualsAround(tile.GridPosition);
            RecalculateTownHallConnections();
        }

        /// <summary>
        /// Usuwa drogę z kafelka, zeruje grafikę i aktualizuje połączenia oraz sąsiadów.
        /// </summary>
        public bool RemoveRoad(TileView tile)
        {
            if (tile == null || !tile.HasRoad) return false;

            _roadTiles.Remove(tile.GridPosition);
            tile.SetHasRoad(false);
            tile.SetRoadSprite(null);

            // Aktualizacja grafiki sąsiadów
            UpdateRoadVisualsAround(tile.GridPosition);

            // Przeliczenie sieci połączeń z Ratuszem
            RecalculateTownHallConnections();

            Debug.Log($"[RoadManager] Usunięto drogę z pozycji {tile.GridPosition}!");
            return true;
        }

        /// <summary>
        /// Czyszczenie całej sieci dróg (np. restart poziomu).
        /// </summary>
        public void ClearNetwork()
        {
            _roadTiles.Clear();
            _connectedToTownHall.Clear();
        }

        // --- 3. AUTO-TILING I SPRAWDZANIE MASKI ---
        public void UpdateRoadVisualsAround(Vector2Int pos)
        {
            UpdateTileRoadSprite(pos);

            // Aktualizacja 4 sąsiadów
            UpdateTileRoadSprite(pos + Vector2Int.up);
            UpdateTileRoadSprite(pos + Vector2Int.right);
            UpdateTileRoadSprite(pos + Vector2Int.down);
            UpdateTileRoadSprite(pos + Vector2Int.left);
        }

        private void UpdateTileRoadSprite(Vector2Int pos)
        {
            if (GridManager.Instance == null) return;

            TileView tile = GridManager.Instance.GetTileAt(pos);
            if (tile == null) return;

            if (!tile.HasRoad)
            {
                tile.SetRoadSprite(null);
                return;
            }

            int mask = GetRoadConnectionMask(pos);
            Sprite selectedSprite = null;
            if (tile.Type == TileType.River || tile.Type == TileType.Ocean)
            {
                _bridgeSpriteLookup.TryGetValue(mask, out selectedSprite);
            }

            // Jeśli brakuje grafiki mostu lub to ląd -> standardowa DROGA
            if (selectedSprite == null)
            {
                _roadSpriteLookup.TryGetValue(mask, out selectedSprite);
            }
            Sprite roadSprite = GetRoadSpriteForMask(mask);
            tile.SetRoadSprite(selectedSprite);
        }

        public int GetRoadConnectionMask(Vector2Int pos)
        {
            int mask = 0;

            if (HasRoadOrTownHall(pos + Vector2Int.up))    mask |= (int)RoadDirections.North; // 1
            if (HasRoadOrTownHall(pos + Vector2Int.right)) mask |= (int)RoadDirections.East;  // 2
            if (HasRoadOrTownHall(pos + Vector2Int.down))  mask |= (int)RoadDirections.South; // 4
            if (HasRoadOrTownHall(pos + Vector2Int.left))  mask |= (int)RoadDirections.West;  // 8

            return mask;
        }

        private bool HasRoadOrTownHall(Vector2Int pos)
        {
            if (GridManager.Instance == null) return false;
            TileView tile = GridManager.Instance.GetTileAt(pos);

            if (tile == null) return false;
            return tile.HasRoad || tile.Type == TileType.TownHall;
        }

        private Sprite GetRoadSpriteForMask(int mask)
        {
            if (_roadSpriteLookup.TryGetValue(mask, out Sprite sprite))
                return sprite;

            Debug.LogWarning($"[RoadManager] Brak sprajta drogi dla maski {mask}!");
            return null;
        }

        // --- 4. SIEĆ POŁĄCZEŃ Z RATUSZEM (BFS) ---
        public void RecalculateTownHallConnections()
        {
            _connectedToTownHall.Clear();

            if (GridManager.Instance == null) return;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            foreach (var roadPos in _roadTiles)
            {
                if (IsAdjacentToTownHall(roadPos))
                {
                    queue.Enqueue(roadPos);
                    _connectedToTownHall.Add(roadPos);
                }
            }

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    if (_roadTiles.Contains(neighbor) && !_connectedToTownHall.Contains(neighbor))
                    {
                        _connectedToTownHall.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        private bool IsAdjacentToTownHall(Vector2Int pos)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            foreach (var dir in directions)
            {
                TileView tile = GridManager.Instance.GetTileAt(pos + dir);
                if (tile != null && tile.Type == TileType.TownHall) return true;
            }
            return false;
        }

        public bool IsRoadConnectedToTownHall(Vector2Int pos)
        {
            return _connectedToTownHall.Contains(pos);
        }

        public bool IsBuildingConnectedToTownHall(Vector2Int buildingPos)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            foreach (var dir in directions)
            {
                Vector2Int neighbor = buildingPos + dir;
                TileView neighborTile = GridManager.Instance.GetTileAt(neighbor);

                if (neighborTile != null && neighborTile.Type == TileType.TownHall)
                    return true;

                if (_connectedToTownHall.Contains(neighbor))
                    return true;
            }

            return false;
        }
    }
}