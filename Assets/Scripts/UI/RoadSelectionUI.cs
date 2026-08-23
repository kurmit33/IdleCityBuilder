using UnityEngine;
using UnityEngine.UI;
using IdleBuilder.World;

namespace IdleBuilder.UI
{
    public class RoadSelectionUI : MonoBehaviour
    {
        [Header("Przyciski Wyboru Ery Dróg")]
        [SerializeField] private Button era1RoadButton; // Ścieżka Ubita
        [SerializeField] private Button era2RoadButton; // Trakt Kamienny
        [SerializeField] private Button era3RoadButton; // Bruk
        [SerializeField] private Button era4RoadButton; // Asfalt
        [SerializeField] private Button era5RoadButton; // Beton
        [SerializeField] private Button era6RoadButton; // Maglev
        [SerializeField] private Button era7RoadButton; // Hyperloop

        private void Start()
        {
            if (era1RoadButton) era1RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Prehistoric));
            if (era2RoadButton) era2RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Antiquity));
            if (era3RoadButton) era3RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.MiddleAges));
            if (era4RoadButton) era4RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Industrial));
            if (era5RoadButton) era5RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Atomic));
            if (era6RoadButton) era6RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Digital));
            if (era7RoadButton) era7RoadButton.onClick.AddListener(() => SelectRoadTier(RoadTier.Fusion));

        }

        public void SelectRoadTier(RoadTier tier)
        {
            RoadNetworkManager.Instance.SetSelectedRoadTier(tier);
            RoadPlacementManager.Instance.SetBuildMode(true);
        }
    }
}