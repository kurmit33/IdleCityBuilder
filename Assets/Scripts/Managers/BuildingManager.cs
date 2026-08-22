using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;
using IdleBuilder.World;
using IdleBuilder.Buildings;

namespace IdleBuilder.Managers
{
    public class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }

        [Header("Aktualnie Wybrany Budynek do Postawienia")]
        [SerializeField] private BuildingData selectedBuilding;

        // Słownik przechowujący postawione budynki na siatce: Pos -> BuildingData
        private readonly Dictionary<Vector2Int, BuildingData> _placedBuildings = new Dictionary<Vector2Int, BuildingData>();

        // Zdarzenie informujące o postawieniu budynku
        public static event Action<Vector2Int, BuildingData> OnBuildingPlaced;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            TileView.OnTileClickedEvent += HandleTileClicked;
        }

        private void OnDisable()
        {
            TileView.OnTileClickedEvent -= HandleTileClicked;
        }

        // --- WYBÓR BUDYNKU W UI ---
        public void SelectBuildingToBuild(BuildingData buildingData)
        {
            selectedBuilding = buildingData;
            Debug.Log($"[BuildingManager] Wybrano do budowy: {buildingData?.buildingName ?? "Brak"}");
        }

        public BuildingData GetSelectedBuilding() => selectedBuilding;

        // --- REAKCJA NA KLIKNIĘCIE W KAFELEK ---
        private void HandleTileClicked(TileView tile)
        {
            // Sprawdzamy czy w PlayerInteractionManager aktywny jest tryb Build
            if (PlayerClickManager.Instance == null || 
                PlayerClickManager.Instance.CurrentMode != PlayerInteractionMode.Build)
                return;

            if (selectedBuilding == null)
            {
                Debug.LogWarning("[BuildingManager] Wybrano tryb budowania, ale nie wyznaczono żadnego budynku z menu!");
                return;
            }

            TryPlaceBuilding(tile, selectedBuilding);
        }

        // --- WALIDACJA POSTAWIENIA BUDYNKU ---
        public bool CanPlaceBuildingAt(TileView tile, BuildingData data, out string failureReason)
        {
            failureReason = string.Empty;

            if (tile == null)
            {
                failureReason = "Brak kafelka.";
                return false;
            }

            // 1. Czy teren jest odblokowany?
            if (!tile.IsUnlocked)
            {
                failureReason = "Teren jest zablokowany!";
                return false;
            }

            // 2. Czy na polu nie stoi już inny budynek ani droga?
            if (_placedBuildings.ContainsKey(tile.GridPosition))
            {
                failureReason = "Na tym polu stoi już inny budynek!";
                return false;
            }

            if (tile.HasRoad)
            {
                failureReason = "Nie można stawiać budynków bezpośrednio na drodze!";
                return false;
            }

            if (tile.Type == TileType.TownHall || tile.Type == TileType.Ocean)
            {
                failureReason = "Nieprawidłowe miejsce pod budowę!";
                return false;
            }

            // 3. Czy typ terenu jest dozwolony dla tego budynku?
            if (!data.allowedTerrains.Contains(tile.Type))
            {
                failureReason = $"Budynek wymaga terenu typu: {string.Join(", ", data.allowedTerrains)}";
                return false;
            }

            // 4. Sprawdzanie zasobów gracza
            if (ResourceManager.Instance != null)
            {
                foreach (var cost in data.buildCosts)
                {
                    if (!ResourceManager.Instance.HasEnough(cost.resource, cost.amount))
                    {
                        failureReason = $"Brak zasobu: {cost.resource} ({cost.amount})";
                        return false;
                    }
                }
            }

            // 5. Weryfikacja połączenia drogowego z Ratuszem
            if (data.requiresRoadConnection && RoadNetworkManager.Instance != null)
            {
                bool isConnected = RoadNetworkManager.Instance.IsBuildingConnectedToTownHall(tile.GridPosition);
                if (!isConnected)
                {
                    failureReason = "Budynek musi przylegać do drogi połączonej z Ratuszem!";
                    return false;
                }
            }

            return true;
        }

        // --- FIZYCZNE POSTAWIENIE BUDYNKU ---
        public bool TryPlaceBuilding(TileView tile, BuildingData data)
        {
            if (!CanPlaceBuildingAt(tile, data, out string reason))
            {
                Debug.LogWarning($"[BuildingManager] Nie można wybudować {data.buildingName}: {reason}");
                return false;
            }

            // Pobranie zasobów z portfela gracza
            if (ResourceManager.Instance != null)
            {
                foreach (var cost in data.buildCosts)
                {
                    ResourceManager.Instance.SubtractResource(cost.resource, cost.amount);
                }
            }

            // Rejestracja budynku na siatce
            _placedBuildings[tile.GridPosition] = data;

            // Ustawienie grafiki budynku na kafelku (lub stworzenie podobiektu)
            tile.SetRoadSprite(data.buildingSprite); // Korzystamy z dedykowanej warstwy sprite'a na kafelku

            OnBuildingPlaced?.Invoke(tile.GridPosition, data);
            Debug.Log($"[BuildingManager] Pomyślnie postawiono {data.buildingName} na pozycji {tile.GridPosition}!");

            return true;
        }

        public bool HasBuildingAt(Vector2Int pos) => _placedBuildings.ContainsKey(pos);
        public BuildingData GetBuildingAt(Vector2Int pos) => _placedBuildings.TryGetValue(pos, out var data) ? data : null;
        public IReadOnlyDictionary<Vector2Int, BuildingData> GetAllBuildings() => _placedBuildings;

        /// <summary> Usunięcie budynku z danej pozycji </summary>
        public bool RemoveBuildingAt(TileView tile)
        {
            if (tile == null) return false;

            if (_placedBuildings.ContainsKey(tile.GridPosition))
            {
                _placedBuildings.Remove(tile.GridPosition);
                tile.SetRoadSprite(null); // Czyszczenie grafiki budynku z kafelka
                
                Debug.Log($"[BuildingManager] Usunięto budynek z pozycji {tile.GridPosition}");
                return true;
            }

            return false;
        }
    }
}