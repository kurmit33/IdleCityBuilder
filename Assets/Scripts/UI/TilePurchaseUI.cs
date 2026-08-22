using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleBuilder.Core;
using IdleBuilder.World;

namespace IdleBuilder.Managers
{
    public class TilePurchaseUI : MonoBehaviour
    {
        [Header("UI Containers")]
        [SerializeField] private GameObject popupPanel;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI eraLimitText;

        [Header("Buttons")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        private TileView _selectedTile;
        private EraType _currentEra = EraType.Prehistory;

        private void Awake()
        {
            if (popupPanel == null)
            {
                Transform found = transform.Find("PurchasePanel");
                if (found != null) popupPanel = found.gameObject;
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }
            else
            {
                Debug.LogError("[TilePurchaseUI] BŁĄD: Przycisk 'Buy Button' NIE JEST przypisany!");
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(HideWindow);
            }
            else
            {
                Debug.LogError("[TilePurchaseUI] BŁĄD: Przycisk 'Close Button' NIE JEST przypisany!");
            }

            HideWindow();
        }

        private void OnEnable()
        {
            TileView.OnTileClickedEvent += HandleTileClicked;
        }

        private void OnDisable()
        {
            TileView.OnTileClickedEvent -= HandleTileClicked;
        }

        private void HandleTileClicked(TileView tile)
        {
            if (tile == null || tile.IsUnlocked) return;

            _selectedTile = tile;
            ShowWindow();
        }

        private void ShowWindow()
        {
            if (_selectedTile == null) return;

            if (popupPanel == null)
            {
                Debug.LogError("[TilePurchaseUI] NIE MOŻNA OTWORZYĆ: Pole 'Popup Panel' jest PUSTE!");
                return;
            }

            popupPanel.SetActive(true);

            // 1. Sprawdzanie przylegania do siatki
            bool isAdjacent = GridManager.Instance != null && GridManager.Instance.IsTileAdjacentToUnlocked(_selectedTile.GridPosition);

            // 2. Sprawdzanie limitów ery i zasobów
            bool hasEraCapacity = ExpansionManager.Instance != null && ExpansionManager.Instance.HasEraCapacity(_currentEra);
            bool hasResources = ExpansionManager.Instance != null && ExpansionManager.Instance.HasEnoughResourcesForExpansion(_currentEra);
            int remainingInEra = ExpansionManager.Instance != null ? ExpansionManager.Instance.GetRemainingExpansionsInEra(_currentEra) : 0;

            if (titleText != null) titleText.text = $"Ziemia: {_selectedTile.Type}";
            if (eraLimitText != null) eraLimitText.text = $"Pozostało rozbudów w erze: {remainingInEra}";

            // 3. Renderowanie listy kosztów (obsługuje od 1 do 8+ zasobów)
            RenderCostList();

            // 4. Weryfikacja statusu i odblokowanie przycisku
            if (statusText != null && buyButton != null)
            {
                if (!isAdjacent)
                {
                    statusText.text = "<color=red>Teren musi przylegać do odblokowanego obszaru!</color>";
                    buyButton.interactable = false;
                }
                else if (!hasEraCapacity)
                {
                    statusText.text = "<color=red>Osiągnięto limit rozbudów dla tej Ery!</color>";
                    buyButton.interactable = false;
                }
                else if (!hasResources)
                {
                    statusText.text = "<color=red>Brak wymaganych zasobów!</color>";
                    buyButton.interactable = false;
                }
                else
                {
                    statusText.text = "<color=green>Teren gotowy do odblokowania!</color>";
                    buyButton.interactable = true;
                }
            }
        }

        private void RenderCostList()
        {
            if (costText == null || ExpansionManager.Instance == null) return;

            var costs = ExpansionManager.Instance.GetExpansionCost(_currentEra);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Wymagane zasoby:</b>");

            foreach (var kvp in costs)
            {
                ResourceType resource = kvp.Key;
                double requiredAmount = kvp.Value;
                double currentAmount = ResourceManager.Instance != null ? ResourceManager.Instance.GetAmount(resource) : 0;

                bool hasEnough = currentAmount >= requiredAmount;
                string colorHex = hasEnough ? "#00FF00" : "#FF4444"; // Zielony jeśli starczy, Czerwony jeśli brakuje

                sb.AppendLine($"<color={colorHex}>• {resource}: {currentAmount}/{requiredAmount}</color>");
            }

            costText.text = sb.ToString();
        }

        private void OnBuyClicked()
        {
            if (_selectedTile == null) return;

            if (ExpansionManager.Instance != null)
            {
                bool success = ExpansionManager.Instance.TryBuyExpansion(_currentEra);

                if (success)
                {
                    _selectedTile.SetUnlockedState(true);
                    Debug.Log($"[TilePurchaseUI] Pomyślnie zakupiono i odblokowano kafelek {_selectedTile.GridPosition}!");
                    HideWindow();
                }
                else
                {
                    Debug.LogWarning("[TilePurchaseUI] Nie udało się dokonać zakupu!");
                    ShowWindow(); // Odśwież UI w razie niepowodzenia
                }
            }
        }

        public void HideWindow()
        {
            _selectedTile = null;
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }
    }
}