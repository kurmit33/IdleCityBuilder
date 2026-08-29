using System;
using UnityEngine;
using System.Collections.Generic;
using IdleBuilder.Core;

namespace IdleBuilder.World
{
    [Serializable]
    public class RoadBonusResult
    {
        public bool isConnectedToTownHall;
        public RoadTier effectiveTier;
        public float productionBonusPercent;
        public float maintenanceDiscountPercent;
    }
    public enum RoadTier
    {
        None = 0,
        Prehistoric = 1,
        Antiquity = 2,
        MiddleAges = 3,
        Industrial = 4,
        Atomic = 5,
        Digital = 6,
        Fusion = 7
    }

    [Serializable]
    public class ResourceCost
    {
        public ResourceType type;
        public int amount;
    }

    [Serializable]
    public class RoadTierConfig
    {
        public RoadTier tier;
        public string tierName;
        public EraType eraRequirement;

        public List<ResourceCost> baseCosts = new List<ResourceCost>();

        public float productionBonusPercent = 5f;
        public float maintenanceDiscountPercent = 0f;

        public string roadFolder;
        public string bridgeFolder;
    }
}