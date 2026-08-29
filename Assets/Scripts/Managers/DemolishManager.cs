using UnityEngine;
using IdleBuilder.World;

namespace IdleBuilder.Managers
{
    public class DemolishManager : MonoBehaviour
    {
        public static DemolishManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool TryDemolish(TileView tile)
        {
            if (tile == null)
                return false;

            // 1. Droga
            if (tile.HasRoad)
            {
                if (RoadManager.Instance != null)
                {
                    return RoadManager.Instance.RemoveRoad(tile);
                }

                return false;
            }

            // ------------------------------------------------
            // W PRZYSZŁOŚCI:
            // ------------------------------------------------

            // Budynek
            // if (tile.HasBuilding)
            // {
            //     return BuildingManager.Instance.RemoveBuilding(tile);
            // }

            // Dekoracja
            // if (tile.HasDecoration)
            // {
            //     return DecorationManager.Instance.RemoveDecoration(tile);
            // }

            return false;
        }
    }
}