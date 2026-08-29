using System.Collections.Generic;

namespace IdleBuilder.Core
{
    public static class GameDataNames
    {
        public static readonly Dictionary<ResourceType, string> ResourceNames =
            new Dictionary<ResourceType, string>
            {
                // ERA 1 — PREHISTORIA
                { ResourceType.Wood, "Drewno" },
                { ResourceType.Stone, "Kamień" },
                { ResourceType.RawFood, "Surowa żywność" },
                { ResourceType.Hides, "Skóry" },

                // ERA 2 — STAROŻYTNOŚĆ
                { ResourceType.CopperOre, "Ruda miedzi" },
                { ResourceType.TinOre, "Ruda cyny" },
                { ResourceType.IronOre, "Ruda żelaza" },
                { ResourceType.Grains, "Zboże" },
                { ResourceType.Clay, "Glina" },

                // ERA 3 — ŚREDNIOWIECZE
                { ResourceType.Lumber, "Drewno konstrukcyjne" },
                { ResourceType.Charcoal, "Węgiel drzewny" },
                { ResourceType.Gold, "Złoto" },
                { ResourceType.Textiles, "Tekstylia" },
                { ResourceType.Limestone, "Wapień" },

                // ERA 4 — PRZEMYSŁ
                { ResourceType.Coal, "Węgiel" },
                { ResourceType.Steel, "Stal" },
                { ResourceType.CrudeOil, "Ropa naftowa" },
                { ResourceType.Rubber, "Guma" },
                { ResourceType.Cotton, "Bawełna" },

                // ERA 5 — ATOM I WSPÓŁCZESNOŚĆ
                { ResourceType.Uranium, "Uran" },
                { ResourceType.RefinedCopper, "Rafinowana miedź" },
                { ResourceType.RefinedFuel, "Paliwo rafinowane" },
                { ResourceType.Plastics, "Tworzywa sztuczne" },
                { ResourceType.Lithium, "Lit" },

                // ERA 6 — CYFROWA
                { ResourceType.Silicon, "Krzem" },
                { ResourceType.RareEarthMetals, "Metale ziem rzadkich" },
                { ResourceType.LiIonBatteries, "Baterie litowo-jonowe" },
                { ResourceType.Data, "Dane" },
                { ResourceType.AdvancedPolymers, "Zaawansowane polimery" },

                // ERA 7 — FUZJA I PRZYSZŁOŚĆ
                { ResourceType.Helium3, "Tryt" },
                { ResourceType.Deuterium, "Deuter" },
                { ResourceType.Graphene, "Grafen" },
                { ResourceType.CarbonNanotubes, "Nanorurki węglowe" },
                { ResourceType.TitaniumAlloys, "Stopy tytanu" },
                { ResourceType.Metamaterials, "Metamateriały" },
                { ResourceType.Superconductors, "Nadprzewodniki" },
                { ResourceType.QuantumCompute, "Obliczenia kwantowe" },
                { ResourceType.SyntheticBiomass, "Biomasa syntetyczna" },
                { ResourceType.Antimatter, "Antymateria" }
            };

        public static readonly Dictionary<EraType, string> EraNames =
            new Dictionary<EraType, string>
            {
                { EraType.Prehistory, "Prehistoria" },
                { EraType.Antiquity, "Starożytność" },
                { EraType.MiddleAges, "Średniowiecze" },
                { EraType.Industrial, "Era przemysłowa" },
                { EraType.Atomic, "Era atomowa" },
                { EraType.Digital, "Era cyfrowa" },
                { EraType.Fusion, "Era fuzji i przyszłości" }
            };

        public static readonly Dictionary<TileType, string> TileNames =
            new Dictionary<TileType, string>
            {
                { TileType.Empty, "Pusty teren" },
                { TileType.Forest, "Las" },
                { TileType.Mountain, "Góry" },
                { TileType.Plains, "Równiny" },
                { TileType.River, "Rzeka" },
                { TileType.Ocean, "Ocean" },
                { TileType.TownHall, "Ratusz" }
            };

        public static string GetResourceName(ResourceType type)
        {
            return ResourceNames.TryGetValue(type, out string name)
                ? name
                : type.ToString();
        }

        public static string GetEraName(EraType type)
        {
            return EraNames.TryGetValue(type, out string name)
                ? name
                : type.ToString();
        }

        public static string GetTileName(TileType type)
        {
            return TileNames.TryGetValue(type, out string name)
                ? name
                : type.ToString();
        }
    }
}