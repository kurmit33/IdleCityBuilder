using System.Collections.Generic;
using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.Buildings
{
    [System.Serializable]
    public struct ResourceAmount
    {
        public ResourceType resource;
        public double amount;
    }

    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "IdleBuilder/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Podstawowe Informacje")]
        public string buildingName = "Nowy Budynek";
        public BuildingType buildingType;
        public EraType requiredEra = EraType.Prehistory;
        public Sprite buildingSprite;

        [Header("Wymagania Terenowe i Droga")]
        public List<TileType> allowedTerrains = new List<TileType> { TileType.Plains };
        public bool requiresRoadConnection = true;

        [Header("Koszty Budowy")]
        public List<ResourceAmount> buildCosts = new List<ResourceAmount>();

        [Header("PRODUKCJA (Przetwórstwo / Pozyskiwanie)")]
        [Tooltip("Zasoby zużywane co sekundę/tick (np. Drewno dla Tartaku)")]
        public List<ResourceAmount> inputResources = new List<ResourceAmount>();
        
        [Tooltip("Zasoby generowane co sekundę/tick (np. Tarcica)")]
        public List<ResourceAmount> outputResources = new List<ResourceAmount>();

        [Header("MIESZKALNICTWO (Używane tylko dla domów)")]
        public bool isHousing = false;
        public int maxResidents = 0;              // Miejsca dla mieszkańców
        public double taxYieldPerSec = 0.0;       // Przychód złota/monet
        public List<ResourceAmount> upkeepCosts = new List<ResourceAmount>(); // np. RawFood/Grains na sekundę
    }
}