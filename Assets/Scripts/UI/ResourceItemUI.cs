using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleBuilder.Core;

namespace IdleBuilder.UI
{
    public class ResourceItemUI : MonoBehaviour
    {
        [SerializeField] private Image resourceIcon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI rateText;

        public ResourceType Type { get; private set; }

        public void Setup(ResourceType type, Sprite icon)
        {
            Type = type;
            if (resourceIcon != null && icon != null)
            {
                resourceIcon.sprite = icon;
            }
        }

        public void UpdateDisplay(double amount, double ratePerSec)
        {
            if (amountText != null)
                amountText.text = NumberFormatter.Format(amount);

            if (rateText != null)
            {
                if (ratePerSec > 0)
                {
                    rateText.text = $"+{NumberFormatter.Format(ratePerSec)}/s";
                    rateText.gameObject.SetActive(true);
                }
                else
                {
                    rateText.gameObject.SetActive(false);
                }
            }
        }
    }
}