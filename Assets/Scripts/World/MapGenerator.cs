using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.World
{
    public static class MapGenerator
    {
        public static TileType[,] GenerateMap(int width, int height)
        {
            TileType[,] map = new TileType[width, height];

            // Losowe przesunięcie szumu Perlina dla unikalności mapy
            float seedX = Random.Range(0f, 1000f);
            float seedY = Random.Range(0f, 1000f);
            float scale = 0.15f; // Im mniejsza wartość, tym większe skupiska terenu

            int centerX = width / 2;
            int centerY = height / 2;

            // 1. Generowanie Biomów na podstawie Szumu Perlina
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sampleX = x * scale + seedX;
                    float sampleY = y * scale + seedY;
                    float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);

                    // Podział progowy wartości szumu na biomy
                    if (noiseValue < 0.25f)
                        map[x, y] = TileType.Ocean;      // Duże skupiska oceanu
                    else if (noiseValue < 0.45f)
                        map[x, y] = TileType.Plains;     // Pola
                    else if (noiseValue < 0.75f)
                        map[x, y] = TileType.Forest;     // Lasy
                    else
                        map[x, y] = TileType.Mountain;   // Góry
                }
            }

            // 2. Generowanie Ciągłej Rzeki
            GenerateRiver(map, width, height, centerX, centerY);

            // 3. GWARANTOWANY STARTER (Krzyż wokół Ratusza)
            // Ustawiamy go na końcu, aby rzeka ani szum go nie nadpisały
            map[centerX, centerY] = TileType.TownHall;                             // Środek -> Ratusz
            if (centerY + 1 < height) map[centerX, centerY + 1] = TileType.Mountain; // Północ -> Góra
            if (centerX - 1 >= 0)     map[centerX - 1, centerY] = TileType.Forest;   // Zachód -> Las
            if (centerY - 1 >= 0)     map[centerX, centerY - 1] = TileType.Plains;   // Południe -> Pole
            if (centerX + 1 < width)  map[centerX + 1, centerY] = TileType.River;    // Wschód -> Rzeka

            return map;
        }

        private static void GenerateRiver(TileType[,] map, int width, int height, int centerX, int centerY)
        {
            // Rzeka startuje z górnego brzegu i idzie na dół
            int rx = Random.Range(1, width - 1);
            int ry = height - 1;

            while (ry >= 0)
            {
                // Omijamy strefę starteru 3x3 w centrum, aby nie psuć krzyża startowego
                bool isStarterZone = Mathf.Abs(rx - centerX) <= 1 && Mathf.Abs(ry - centerY) <= 1;
                if (!isStarterZone)
                {
                    map[rx, ry] = TileType.River;
                }

                // Rzeka zakręca losowo w lewo lub w prawo, ale zawsze dąży w dół
                int dir = Random.Range(-1, 2); // -1 (lewo), 0 (prosto), 1 (prawo)
                rx = Mathf.Clamp(rx + dir, 0, width - 1);
                ry--;
            }
        }
    }
}