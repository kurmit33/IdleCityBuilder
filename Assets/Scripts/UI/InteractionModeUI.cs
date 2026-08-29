using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IdleBuilder.World;
using IdleBuilder.Managers; // Prawidłowy dostęp do PlayerInteractionMode

namespace IdleBuilder.UI
{
    public class InteractionModeUI : MonoBehaviour
    {
        [System.Serializable]
        public struct ModeButtonEntry
        {
            public PlayerInteractionMode mode;
            public Button button;
        }

        private class ButtonVisualData
        {
            public RectTransform rectTransform;
            public Image buttonImage;
        }

        [Header("Mode Buttons Setup")]
        [SerializeField] private List<ModeButtonEntry> modeButtons;

        [Header("Animation Settings")]
        [SerializeField] private Vector3 activeScale = new Vector3(1.18f, 1.18f, 1f);
        [SerializeField] private Vector3 inactiveScale = Vector3.one;
        [SerializeField] private float animationSpeed = 12f;

        [Header("Color / Grayout Settings")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);

        private readonly Dictionary<PlayerInteractionMode, ButtonVisualData> _visualData = new Dictionary<PlayerInteractionMode, ButtonVisualData>();
        private PlayerInteractionMode _activeMode = PlayerInteractionMode.Gather;

        private void Awake()
        {
            foreach (var entry in modeButtons)
            {
                if (entry.button != null)
                {
                    Image img = entry.button.GetComponent<Image>();
                    if (img == null)
                    {
                        img = entry.button.GetComponentInChildren<Image>();
                    }

                    _visualData[entry.mode] = new ButtonVisualData
                    {
                        rectTransform = entry.button.GetComponent<RectTransform>(),
                        buttonImage = img
                    };

                    PlayerInteractionMode selectedMode = entry.mode;
                    entry.button.onClick.AddListener(() => SwitchMode(selectedMode));
                }
            }
        }

        private void Start()
        {
            SwitchMode(PlayerInteractionMode.Gather);
        }

        private void Update()
        {
            foreach (var kvp in _visualData)
            {
                var data = kvp.Value;
                if (data == null) continue;

                bool isActive = (kvp.Key == _activeMode);

                Vector3 targetScale = isActive ? activeScale : inactiveScale;
                if (data.rectTransform != null)
                {
                    data.rectTransform.localScale = Vector3.Lerp(data.rectTransform.localScale, targetScale, Time.deltaTime * animationSpeed);
                }

                if (data.buttonImage != null)
                {
                    Color targetColor = isActive ? activeColor : inactiveColor;
                    data.buttonImage.color = Color.Lerp(data.buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
                }
            }
        }

        private void SwitchMode(PlayerInteractionMode mode)
        {
            _activeMode = mode;

            if (PlayerClickManager.Instance != null)
            {
                PlayerClickManager.Instance.SetInteractionMode(mode);

                if (mode == PlayerInteractionMode.BuildRoad)
                {
                    RoadUI.Instance.ShowRoadTierPanel();
                    TileView.RefreshCurrentHover();
                } else
                {
                    RoadUI.Instance.HideRoadTierPanel();
                }
            }
        }
    }
}