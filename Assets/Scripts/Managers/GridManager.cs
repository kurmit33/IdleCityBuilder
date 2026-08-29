using System.Collections.Generic;
using System;
using UnityEngine;
using IdleBuilder.Core;

namespace IdleBuilder.World
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        [Serializable]
        public class TileVisualData
        {
            public TileType type;
            public Sprite sprite;
        }

        [Header("Grid Settings")]
        [SerializeField] private int width = 21;
        [SerializeField] private int height = 21;
        [SerializeField] private float tileSize = 1.1f;

        [Header("Prefabs")]
        [SerializeField] private TileView tilePrefab;

        [Tooltip("Przypisz ikony/sprity do poszczególnych typów biomów")]
        [SerializeField] private List<TileVisualData> tileVisuals = new List<TileVisualData>();
        private readonly Dictionary<Vector2Int, TileView> _tiles = new Dictionary<Vector2Int, TileView>();
        private readonly Dictionary<TileType, Sprite> _spriteDict = new Dictionary<TileType, Sprite>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            BuildSpriteDictionary();
        }
        private void BuildSpriteDictionary()
        {
            _spriteDict.Clear();
            foreach (var visual in tileVisuals)
            {
                if (!_spriteDict.ContainsKey(visual.type))
                {
                    _spriteDict.Add(visual.type, visual.sprite);
                }
            }
        }

        private void Start()
        {
            GenerateInitialGrid(width, height);
        }

        public void GenerateInitialGrid(int w, int h)
        {
            // Usunięcie starych kafelków
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            _tiles.Clear();

            int centerX = w / 2;
            int centerY = h / 2;

            // Wywołujemy statyczny MapGenerator!
            TileType[,] mapData = MapGenerator.GenerateMap(w, h);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    TileType typeToAssign = mapData[x, y];

                    Vector3 worldPos = GetWorldPosition(x, y, w, h);
                    TileView tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                    Sprite tileSprite = GetSpriteForType(typeToAssign);
                    

                    // Odblokowujemy starter (Ratusz + 4 kafelki wokół)
                    bool isStartingTile = Mathf.Abs(x - centerX) <= 1 && Mathf.Abs(y - centerY) <= 1;
                    tile.Setup(pos, typeToAssign, tileSprite, isStartingTile);
                    tile.SetUnlockedState(isStartingTile);

                    _tiles[pos] = tile;
                }
            }

            Debug.Log($"[GridManager] Pomyślnie wygenerowano siatkę {w}x{h} z MapGeneratora.");
        }

        public Vector3 GetWorldPosition(int x, int y, int gridW, int gridH)
        {
            float offsetX = (gridW * tileSize) / 2f - (tileSize / 2f);
            float offsetY = (gridH * tileSize) / 2f - (tileSize / 2f);

            return new Vector3(x * tileSize - offsetX, y * tileSize - offsetY, 0f);
        }

        public bool IsTileAdjacentToUnlocked(Vector2Int pos)
        {
            Vector2Int[] neighbors = {
                new Vector2Int(pos.x + 1, pos.y),
                new Vector2Int(pos.x - 1, pos.y),
                new Vector2Int(pos.x, pos.y + 1),
                new Vector2Int(pos.x, pos.y - 1)
            };

            foreach (var n in neighbors)
            {
                if (_tiles.TryGetValue(n, out TileView tile))
                {
                    if (tile.IsUnlocked) return true;
                }
            }
            return false;
        }

        public TileView GetTileAt(Vector2Int pos)
        {
            if (_tiles.TryGetValue(pos, out TileView tile)) return tile;
            return null;
        }
        public IEnumerable<TileView> GetAllTiles()
        {
            return _tiles.Values;
        }
        private Sprite GetSpriteForType(TileType type)
        {
            if (_spriteDict.TryGetValue(type, out Sprite sprite))
            {
                return sprite;
            }

            Debug.LogWarning($"[GridManager] Brak przypisanego Sprite'a dla typ biomu: {type}! Sprawdź ustawienia w Inspektorze.");
            return null;
        }
    }

}