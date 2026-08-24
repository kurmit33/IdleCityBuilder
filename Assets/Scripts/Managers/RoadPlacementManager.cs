using System;
using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;
using IdleBuilder.Managers;

namespace IdleBuilder.World
{
    public class RoadPlacementManager : MonoBehaviour
    {
        public static RoadPlacementManager Instance { get; private set; }

        [Header("Ustawienia Trybu")]
        [SerializeField] private bool isBuildModeActive = false;

        private TileView _currentHoveredTile;
        private SpriteRenderer _previewRenderer;

        public event Action<TileView, List<ResourceCost>, bool> OnTileHovered;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            CreatePreviewObject();
        }

        private void CreatePreviewObject()
        {
            GameObject previewObj = new GameObject("RoadPreview");
            _previewRenderer = previewObj.AddComponent<SpriteRenderer>();
            _previewRenderer.sortingOrder = 10;
            _previewRenderer.color = new Color(1f, 1f, 1f, 0.6f);
            previewObj.SetActive(false);
        }

        private void Update()
        {
            if (!isBuildModeActive) return;

            HandleMouseHover();
        }

        public void SetBuildMode(bool active)
        {
            isBuildModeActive = active;
            _previewRenderer.gameObject.SetActive(active);
            if (!active) OnTileHovered?.Invoke(null, null, false);
        }

        private void HandleMouseHover()
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(mouseWorldPos.x), Mathf.RoundToInt(mouseWorldPos.y));

            TileView tile = GridManager.Instance != null ? GridManager.Instance.GetTileAt(gridPos) : null;

            if (tile != _currentHoveredTile)
            {
                _currentHoveredTile = tile;
                UpdatePreviewVisuals();
            }
        }

        private void UpdatePreviewVisuals()
        {
            if (_currentHoveredTile == null || !_currentHoveredTile.IsUnlocked || _currentHoveredTile.Type == TileType.TownHall)
            {
                _previewRenderer.gameObject.SetActive(false);
                OnTileHovered?.Invoke(null, null, false);
                return;
            }

            _previewRenderer.gameObject.SetActive(true);
            _previewRenderer.transform.position = _currentHoveredTile.transform.position;

            int activeMask = RoadNetworkManager.Instance.GetRoadConnectionMask(_currentHoveredTile.GridPosition);

            RoadTier selectedTier = RoadNetworkManager.Instance.CurrentSelectedTier;
            Sprite previewSprite =
                RoadNetworkManager.Instance.GetSpriteFor(
                    selectedTier,
                    activeMask,
                    _currentHoveredTile.Type
                );

            if (previewSprite == null)
            {
                Debug.LogWarning(
                    $"[RoadPreview] Brak sprite'a! " +
                    $"Tier={selectedTier}, " +
                    $"Mask={activeMask}, " +
                    $"Tile={_currentHoveredTile.Type}"
                );

                _previewRenderer.gameObject.SetActive(false);
            }
            else
            {
                _previewRenderer.sprite = previewSprite;
            }

            List<ResourceCost> calculatedCosts = CalculateCostForTile(_currentHoveredTile, selectedTier);
            bool canAfford = HasEnoughResources(calculatedCosts);

            _previewRenderer.color = canAfford ? new Color(0.2f, 1f, 0.2f, 0.7f) : new Color(1f, 0.2f, 0.2f, 0.7f);

            OnTileHovered?.Invoke(_currentHoveredTile, calculatedCosts, canAfford);
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

        private bool HasEnoughResources(List<ResourceCost> costs)
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

            if (!HasEnoughResources(costs)) return  false;

            if (ResourceManager.Instance != null)
            {
                foreach (var cost in costs)
                {
                    ResourceManager.Instance.SubtractResource(cost.type, cost.amount);
                }
            }

            int selectedMask = RoadNetworkManager.Instance.GetRoadConnectionMask(_currentHoveredTile.GridPosition);
            bool success = RoadNetworkManager.Instance.PlaceRoad(tile);
            if (!success)
            {
                return false;
            }
            _currentHoveredTile = null;

            _previewRenderer.gameObject.SetActive(false);

            OnTileHovered?.Invoke(
                null,
                null,
                false
            );
            UpdatePreviewVisuals();
            return true;            

        }
    }
}