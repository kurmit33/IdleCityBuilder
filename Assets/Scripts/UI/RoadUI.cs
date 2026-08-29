using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Text;
using TMPro;
using IdleBuilder.Core;
using IdleBuilder.World;
using IdleBuilder.Managers;
using System.Collections.Generic;

namespace IdleBuilder.UI
{
    public class RoadUI : MonoBehaviour
    {
        public static RoadUI Instance { get; private set; }
        public RoadTier SelectedRoadTier = RoadTier.Prehistoric;
        [Header("UI Panel Reference")]
        [SerializeField] private GameObject tooltipPanel;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI tileTypeTitleText;
        [SerializeField] private TextMeshProUGUI costTextText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Settings")]
        [Tooltip("Czy dymek ma podążać za kursorem myszy")]
        [SerializeField] private bool followMouse = true;
        [SerializeField] private Vector3 mouseOffset = new Vector3(25f, -25f, 0f);
        [Header("Road Preview")]
        [SerializeField] private SpriteRenderer previewRenderer;

        [Header("Road Sprites")]
        private readonly Dictionary<RoadTier, Dictionary<int, Sprite>> _roadSpritesByTier = new Dictionary<RoadTier, Dictionary<int, Sprite>>();
        private readonly Dictionary<RoadTier, Dictionary<int, Sprite>> _bridgeSpritesByTier = new Dictionary<RoadTier, Dictionary<int, Sprite>>();
                [Header("Przyciski Wyboru Ery Dróg")]
        [SerializeField] private Button era1RoadButton; // Ścieżka Ubita
        [SerializeField] private Button era2RoadButton; // Trakt Kamienny
        [SerializeField] private Button era3RoadButton; // Bruk
        [SerializeField] private Button era4RoadButton; // Asfalt
        [SerializeField] private Button era5RoadButton; // Beton
        [SerializeField] private Button era6RoadButton; // Maglev
        [SerializeField] private Button era7RoadButton; // Hyperloop


  

        private RectTransform _rectTransform;
        private Canvas _parentCanvas;



        private void Awake()
        {
            if (tooltipPanel != null)
            {
                _rectTransform = tooltipPanel.GetComponent<RectTransform>();
                _parentCanvas = GetComponentInParent<Canvas>();
            }

            if (previewRenderer == null)
            {
                GameObject previewObj = new GameObject("RoadPreview");
                previewRenderer = previewObj.AddComponent<SpriteRenderer>();
            }

            previewRenderer.sortingOrder = 10;
            previewRenderer.color = new Color(1f, 1f, 1f, 0.6f);
            previewRenderer.gameObject.SetActive(false);
            HideTooltip();
        }

        private void Start()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            LoadAllRoadSprites();
            if (era1RoadButton) era1RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Prehistoric));
            if (era2RoadButton) era2RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Antiquity));
            if (era3RoadButton) era3RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.MiddleAges));
            if (era4RoadButton) era4RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Industrial));
            if (era5RoadButton) era5RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Atomic));
            if (era6RoadButton) era6RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Digital));
            if (era7RoadButton) era7RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Fusion));

        
        }

        private void OnEnable()
        {
            TileView.OnTileHoveredEvent += HandleTileHovered;
            TileView.OnTileUnhoveredEvent += HideAll;
        }

        private void OnDisable()
        {
            TileView.OnTileHoveredEvent -= HandleTileHovered;
            TileView.OnTileUnhoveredEvent -= HideAll;
        }

        private void Update()
        {
            if (followMouse && tooltipPanel != null && tooltipPanel.activeSelf)
            {
                UpdatePositionToMouse();
            }
        }

        public void UpdateRoadPreview(TileView tile)
        {
            if (tile == null || !tile.IsUnlocked || tile.Type == TileType.TownHall )
            {
                HideRoadPreview();
                return;
            }
            if(tile.HasRoad && tile.RoadTier <= SelectedRoadTier)
            {
                HideRoadPreview();
                return;
            }

            if (RoadManager.Instance == null)
            {
                HideRoadPreview();
                return;
            }

            int mask = RoadManager.Instance.GetRoadConnectionMask(
                tile.GridPosition
            );

            Sprite previewSprite = GetRoadSprite(
                SelectedRoadTier,
                mask,
                tile.Type
            );

            if (previewSprite == null)
            {
                HideRoadPreview();
                return;
            }

            previewRenderer.gameObject.SetActive(true);
            previewRenderer.transform.position = tile.transform.position;
            previewRenderer.sprite = previewSprite;
        }

        public void HideAll()
        {
            HideTooltip();
            HideRoadPreview();
        }
        public void HideRoadPreview()
        {

            if (previewRenderer != null)
            {
                previewRenderer.gameObject.SetActive(false);
            }
        }
        public void ApplyRoadMask(TileView tile, int mask)
        {
            if (tile == null || RoadManager.Instance == null)
                return;
            RoadTier tier = tile.RoadTier;

            Sprite roadSprite = GetRoadSprite(tier, mask, tile.Type);

            if (roadSprite == null)
                return;

            tile.SetRoadSprite(roadSprite);
        }

        public void SetRoadPreviewActive(bool active)
        {
            if (previewRenderer != null)
                previewRenderer.gameObject.SetActive(active);
        }

        private void HandleTileHovered(TileView tile)
        {
            if (PlayerClickManager.Instance == null || 
                PlayerClickManager.Instance.CurrentMode != PlayerInteractionMode.BuildRoad)
            {
                HideTooltip();
                HideRoadPreview();
                return;
            }

            if (tile == null || !tile.IsUnlocked || tile.Type == TileType.TownHall)
            {
                HideTooltip();
                HideRoadPreview();
                return;
            }
            if(tile.HasRoad && tile.RoadTier <= SelectedRoadTier)
            {
                HideTooltip();
                HideRoadPreview();
                return;
            }

            UpdateRoadPreview(tile);
            ShowTooltip(tile);

        }

        private void ShowTooltip(TileView tile)
        {
            StringBuilder costText = new StringBuilder();
            
            if (RoadManager.Instance == null || RoadManager.Instance == null)
            {
                HideTooltip();
                return;
            }
            
            RoadTier tier = SelectedRoadTier;
            List<ResourceCost> costs = RoadManager.Instance.CalculateCostForTile(tile, tier);

            foreach (var cost in costs)
            {
                double currentAmount = 0;
                if (ResourceManager.Instance != null)
                {
                    currentAmount = ResourceManager.Instance.GetAmount(cost.type);
                    string resourceName = GameDataNames.GetResourceName(cost.type);
                    if (ResourceManager.Instance != null)
                    {
                        currentAmount = ResourceManager.Instance.GetAmount(cost.type);
                    }

                    bool enough = currentAmount >= cost.amount;

                    // =====================================================
                    // IKONA ZASOBU - PRZYGOTOWANE POD PRZYSZŁOŚĆ
                    // Przykład:
                    // string icon = GetResourceIcon(cost.type);
                    // costText.Append($"{icon} ");
                    // =====================================================
                    string amountColor = enough ? "green" : "red";

                    costText.AppendLine(
                        $"{resourceName}: <color={amountColor}>{cost.amount}</color> / {currentAmount:0}"
                    );
                }
            }

            if (RoadManager.Instance.HasEnoughResourcesForRoad(costs))
            {
                statusText.text = "<color=green>Kliknij, aby zbudować drogę</color>";
            }
            else
            {
                statusText.text = "<color=red>Brak wystarczających zasobów</color>";
            }
            if (costTextText != null)
            {
                costTextText.text = costText.ToString();
            }

            if (RoadManager.Instance.HasEnoughResourcesForRoad(costs))
            {
                statusText.text =
                    "<color=green>Kliknij, aby zbudować drogę</color>";
            }
            else
            {
                statusText.text =
                    "<color=red>Brak wystarczających zasobów</color>";
            }

            if (tooltipPanel == null) return;
            tooltipPanel.SetActive(true);

            if (tileTypeTitleText != null)
            {
                tileTypeTitleText.text =
                    $"Budowa drogi ({GameDataNames.GetTileName(tile.Type)})";
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


        private Sprite GetRoadSprite(RoadTier tier, int mask, TileType tileType)
        {
            bool bridge = tileType == TileType.River || tileType == TileType.Ocean;
            Dictionary<RoadTier, Dictionary<int, Sprite>> sprites =bridge? _bridgeSpritesByTier: _roadSpritesByTier;

            if (sprites.TryGetValue(tier, out Dictionary<int, Sprite> tierSprites) &&
                tierSprites.TryGetValue(mask, out Sprite sprite))
            {
                return sprite;
            }

            return null;
        }

        private void LoadAllRoadSprites()
        {
            _roadSpritesByTier.Clear();
            _bridgeSpritesByTier.Clear();

            if (RoadManager.Instance == null)
                return;

            foreach (RoadTierConfig config in RoadManager.Instance.GetAllConfigs())
            {
                Dictionary<int, Sprite> roadSprites = new Dictionary<int, Sprite>();
                Dictionary<int, Sprite> bridgeSprites = new Dictionary<int, Sprite>();

                string eraFolder = GetEraFolder(config.tier);

                for (int mask = 0; mask <= 15; mask++)
                {
                    Sprite road = Resources.Load<Sprite>(
                        $"Graphics/Road/{eraFolder}/road_{mask}"
                    );

                    if (road != null)
                        roadSprites[mask] = road;

                    Sprite bridge = Resources.Load<Sprite>(
                        $"Graphics/Bridge/{eraFolder}/bridge_{mask}"
                    );

                    if (bridge != null)
                        bridgeSprites[mask] = bridge;
                }

                _roadSpritesByTier[config.tier] = roadSprites;
                _bridgeSpritesByTier[config.tier] = bridgeSprites;
            }
        }

        public void SelectRoadTier(RoadTier tier)
        {
            if (SelectedRoadTier == tier) return;
            if (tier < RoadTier.Prehistoric || tier > RoadTier.Fusion) return;
            SelectedRoadTier = tier;
        }

        private string GetEraFolder(RoadTier tier)
        {
            switch (tier)
            {
                case RoadTier.Prehistoric:
                    return "Era_1";

                case RoadTier.Antiquity:
                    return "Era_2";

                case RoadTier.MiddleAges:
                    return "Era_3";

                case RoadTier.Industrial:
                    return "Era_4";

                case RoadTier.Atomic:
                    return "Era_5";

                case RoadTier.Digital:
                    return "Era_6";

                case RoadTier.Fusion:
                    return "Era_7";

                default:
                    return "Era_1";
            }
        }
        
    }
}