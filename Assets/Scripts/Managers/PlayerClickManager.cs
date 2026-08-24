using UnityEngine;
using IdleBuilder.World;
using IdleBuilder.Core;
using IdleBuilder.UI;

namespace IdleBuilder.Managers
{
    public enum PlayerInteractionMode
    {
        Gather,    // Ręczne zbieranie
        Build,     // Budowanie
        BuildRoad, // Budowanie dróg
        Demolish   // Niszczenie / Wyburzanie
    }
    public class PlayerClickManager : MonoBehaviour
    {
        public static PlayerClickManager Instance { get; private set; }

        public PlayerInteractionMode CurrentMode { get; private set; } = PlayerInteractionMode.Gather;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnEnable()
        {
            TileView.OnTileClickedEvent += OnTileClicked;
        }

        private void OnDisable()
        {
            TileView.OnTileClickedEvent -= OnTileClicked;
        }

        public void SetInteractionMode(PlayerInteractionMode mode)
        {
            CurrentMode = mode;
        }

        private void OnTileClicked(TileView tile)
        {
            if (tile == null) return;

            // =========================================================================
            // WYJĄTEK DLA RATUSZA (TOWN HALL)
            // Ratusz zignoruje wszystkie narzędzia (Budowanie, Drogi, Wyburzanie, Zbieranie)
            // =========================================================================
            if (tile.Type == TileType.TownHall)
            {
                OpenTownHallWindow();
                return;
            }

            // 1. ZABLOKOWANY kafelek -> obsługa w oknie zakupu terenu
            if (!tile.IsUnlocked) return;

            // 2. ODBLOKOWANY kafelek -> obsługa narzędzi wg trybu
            switch (CurrentMode)
            {
                case PlayerInteractionMode.Gather:
                    HandleGathering(tile);
                    break;

                case PlayerInteractionMode.Build:
                    HandleBuilding(tile);
                    break;

                case PlayerInteractionMode.BuildRoad:
                    HandleRoadBuilding(tile);
                    break;

                case PlayerInteractionMode.Demolish:
                    HandleDemolition(tile);
                    break;
            }
        }

        private void OpenTownHallWindow()
        {
            Debug.Log("[PlayerClickManager] Otwieram Okno Centrum Miasta (Ratusz)!");
            
            // Wywołujemy UI Ratusza z technologiami, erami i statystykami
            //if (TownHallUI.Instance != null)
            //{
            //    TownHallUI.Instance.ShowWindow();
            //}
            //else
            //{
            //    Debug.LogWarning("[PlayerClickManager] Brak instancji TownHallUI na scenie!");
            //}
        }

        private void HandleGathering(TileView tile)
        {
            ResourceType targetResource = ResourceType.Wood;
            int amount = 1;

            switch (tile.Type)
            {
                case TileType.Forest:
                    targetResource = ResourceType.Wood;
                    break;
                case TileType.Mountain:
                    targetResource = ResourceType.Stone;
                    break;
                case TileType.Plains:
                    targetResource = ResourceType.RawFood;
                    break;
                case TileType.River:
                case TileType.Ocean:
                    targetResource = ResourceType.RawFood; // Ryby z rzeki
                    break;
                default:
                    return;
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResource(targetResource, amount);
                //Debug.Log($"[Gather] Wydobyto: +{amount} {targetResource}!");
            }
        }

        private void HandleBuilding(TileView tile)
        {
            Debug.Log($"[Build] Otwieram menu budowy dla kafelka {tile.GridPosition}");
        }

        private void HandleDemolition(TileView tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("[Demolish] Próba wyburzenia pustego kafelka.");
                return;
            }
            // Zabezpieczenie przed zniszczeniem kafelka Ratusza (jeśli budynek leży na kafelku)
            if (tile.Type == TileType.TownHall)
            {
                Debug.LogWarning("[Demolish] Nie można wyburzyć Ratusza!");
                return;
            }
            if (DemolishManager.Instance == null)
            {
                Debug.LogError(
                    "[Demolish] DemolishManager.Instance == null! " +
                    "Sprawdź, czy obiekt z komponentem DemolishManager " +
                    "znajduje się w scenie i jest aktywny."
                );

                return;
            }
            DemolishManager.Instance.TryDemolish(tile);
            
            //Debug.Log($"[Demolish] Wyburzam z kafelka {tile.GridPosition}");
        }

        private void HandleRoadBuilding(TileView tile)
        {

            if (RoadPlacementManager.Instance == null || RoadNetworkManager.Instance == null)
            {
                Debug.LogError(
                    "[Road] Brak RoadPlacementManager lub RoadNetworkManager!"
                );

                return;
            }
            bool success = RoadPlacementManager.Instance.TryBuildRoadOnTile(tile);
            if (success)
            {
                        // Sprawdź, czy nowo postawiona droga łączy się z Ratuszem
                        bool isConnected = RoadNetworkManager.Instance.IsRoadConnectedToTownHall(tile.GridPosition);
                        //Debug.Log($"[Road] Droga na {tile.GridPosition} | Połączona z Ratuszem: {isConnected}");
            }
                
        }
    }
}