using UnityEngine;
using IdleBuilder.UI;

namespace IdleBuilder.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private ResourceBarUI resourceBarUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Miejsce na globalne metody UI, np. UIManager.Instance.ShowEventPopup(...)
    }
}