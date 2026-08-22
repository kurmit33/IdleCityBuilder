using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using IdleBuilder.Core;
using IdleBuilder.World;
using IdleBuilder.Managers;

namespace IdleBuilder.UI
{
    public class RoadCostUI : MonoBehaviour
    {
        [Header("UI Panel Reference")]
        [SerializeField] private GameObject tooltipPanel;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI tileTypeTitleText;
        [SerializeField] private TextMeshProUGUI woodCostText;
        [SerializeField] private TextMeshProUGUI stoneCostText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Settings")]
        [Tooltip("Czy dymek ma podążać za kursorem myszy")]
        [SerializeField] private bool followMouse = true;
        [SerializeField] private Vector3 mouseOffset = new Vector3(25f, -25f, 0f);

        private RectTransform _rectTransform;
        private Canvas _parentCanvas;

        private void Awake()
        {
            if (tooltipPanel != null)
            {
                _rectTransform = tooltipPanel.GetComponent<RectTransform>();
                _parentCanvas = GetComponentInParent<Canvas>();
            }

            HideTooltip();
        }

        private void OnEnable()
        {
            TileView.OnTileHoveredEvent += HandleTileHovered;
            TileView.OnTileUnhoveredEvent += HideTooltip;
        }

        private void OnDisable()
        {
            TileView.OnTileHoveredEvent -= HandleTileHovered;
            TileView.OnTileUnhoveredEvent -= HideTooltip;
        }

        private void Update()
        {
            if (followMouse && tooltipPanel != null && tooltipPanel.activeSelf)
            {
                UpdatePositionToMouse();
            }
        }

        private void HandleTileHovered(TileView tile)
        {
            // 1. Sprawdzamy, czy gracz jest obecnie w trybie budowania dróg
            if (PlayerClickManager.Instance == null || 
                PlayerClickManager.Instance.CurrentMode != PlayerInteractionMode.BuildRoad)
            {
                HideTooltip();
                return;
            }

            // 2. Walidacja kafelka (musi być odblokowany i bez istniejącej drogi)
            if (tile == null || !tile.IsUnlocked || tile.HasRoad)
            {
                HideTooltip();
                return;
            }

            // 3. Pobieramy koszty dla danego kafelka
            if (RoadNetworkManager.Instance != null && 
                RoadNetworkManager.Instance.CanBuildRoadAt(tile, out int woodCost, out int stoneCost))
            {
                ShowTooltip(tile, woodCost, stoneCost);
            }
            else
            {
                HideTooltip();
            }
        }

        private void ShowTooltip(TileView tile, int woodCost, int stoneCost)
        {
            if (tooltipPanel == null) return;

            tooltipPanel.SetActive(true);

            // Tytuł terenu
            if (tileTypeTitleText != null)
            {
                tileTypeTitleText.text = $"Budowa drogi ({GetTerrainName(tile.Type)})";
            }

            // Sprawdzanie zasobów gracza
            bool hasWood = ResourceManager.Instance != null && ResourceManager.Instance.HasEnough(ResourceType.Wood, woodCost);
            bool hasStone = ResourceManager.Instance != null && ResourceManager.Instance.HasEnough(ResourceType.Stone, stoneCost);

            double currentWood = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(ResourceType.Wood) : 0;
            double currentStone = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(ResourceType.Stone) : 0;

            // Formatowanie tekstu drewna
            if (woodCostText != null)
            {
                string woodColor = hasWood ? "#00FF00" : "#FF4444";
                woodCostText.text = $"<color={woodColor}>Drewno: {currentWood}/{woodCost}</color>";
            }

            // Formatowanie tekstu kamienia
            if (stoneCostText != null)
            {
                string stoneColor = hasStone ? "#00FF00" : "#FF4444";
                stoneCostText.text = $"<color={stoneColor}>Kamień: {currentStone}/{stoneCost}</color>";
            }

            // Status zbiorczy
            if (statusText != null)
            {
                if (hasWood && hasStone)
                {
                    statusText.text = "<color=green>Kliknij, aby zbudować drogę</color>";
                }
                else
                {
                    statusText.text = "<color=red>Brak wystarczających zasobów</color>";
                }
            }

            UpdatePositionToMouse();
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void UpdatePositionToMouse()
        {
            if (_rectTransform == null) return;

            Vector2 mousePos = Vector2.zero;
            if (Mouse.current != null)
            {
                mousePos = Mouse.current.position.ReadValue();
            }


            if (_parentCanvas != null && _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _rectTransform.position = mousePos + (Vector2)mouseOffset;
            }
            else if (_parentCanvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentCanvas.transform as RectTransform,
                    mousePos,
                    _parentCanvas.worldCamera,
                    out Vector2 localPoint
                );
                _rectTransform.localPosition = localPoint + (Vector2)mouseOffset;
            }
        }

        private string GetTerrainName(TileType type)
        {
            switch (type)
            {
                case TileType.Plains: return "Równiny";
                case TileType.Forest: return "Las (Wycinka)";
                case TileType.Mountain: return "Góry (Wyrównanie)";
                case TileType.River: return "Rzeka (Most)";
                default: return type.ToString();
            }
        }
    }
}