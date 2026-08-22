using System;
using UnityEngine;
using UnityEngine.EventSystems; // Wymagane do wykrywania UI!
using IdleBuilder.Core;

namespace IdleBuilder.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class TileView : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public TileType Type { get; private set; }
        public bool IsUnlocked { get; private set; }
        public bool HasRoad { get; private set; }


        public static event Action<TileView> OnTileClickedEvent;
        public static event Action<TileView> OnTileHoveredEvent;
        public static event Action OnTileUnhoveredEvent;

        [Header("Road Renderer Reference")]
        [SerializeField] private SpriteRenderer roadSpriteRenderer;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();

            if (roadSpriteRenderer == null)
            {
                GameObject roadChild = new GameObject("RoadRenderer");
                roadChild.transform.SetParent(transform);
                roadChild.transform.localPosition = Vector3.zero;
                roadSpriteRenderer = roadChild.AddComponent<SpriteRenderer>();
                roadSpriteRenderer.sortingOrder = _spriteRenderer.sortingOrder + 1; // Drogę rysujemy NAD terenem
            }
        }
        public void Initialize(Vector2Int gridPosition, TileType type, Sprite sprite, bool isUnlocked)
        {
            GridPosition = gridPosition;
            Type = type;

            UpdateColliderSize();
        }

        public void Setup(Vector2Int gridPosition, TileType type, Sprite sprite, bool isUnlocked)
        {
            GridPosition = gridPosition;
            Type = type;
            if(_spriteRenderer != null)_spriteRenderer.sprite = sprite;

            if (sprite != null && _boxCollider != null)
            {
                _boxCollider.size = sprite.bounds.size;
                _boxCollider.offset = sprite.bounds.center;
            }
            UpdateColliderSize();
            SetUnlockedState(isUnlocked);
        }
        private void UpdateColliderSize()
        {
            if (_spriteRenderer != null && _spriteRenderer.sprite != null && _boxCollider != null)
            {
                _boxCollider.size = _spriteRenderer.sprite.bounds.size;
                _boxCollider.offset = _spriteRenderer.sprite.bounds.center;
            }
        }

        public void SetUnlockedState(bool unlocked)
        {
            IsUnlocked = unlocked;
            if(_spriteRenderer != null)
            {
                if (IsUnlocked)
                {
                    _spriteRenderer.color = Color.white;
                }
                else
                {
                    _spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);
                }
            }

        }

        public void SetHasRoad(bool hasRoad)
        {
            HasRoad = hasRoad;
            if (!HasRoad && roadSpriteRenderer != null)
            {
                roadSpriteRenderer.sprite = null;
            }
        }

        public void SetRoadSprite(Sprite sprite)
        {
            if (roadSpriteRenderer != null)
            {
                roadSpriteRenderer.sprite = sprite;
                roadSpriteRenderer.color = IsUnlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.7f);
            }
        }

        public void OnTileClicked()
        {
            Debug.Log($"[TileView] Kliknięto kafelek: {GridPosition} | Typ: {Type} | Odblokowany: {IsUnlocked}");
            OnTileClickedEvent?.Invoke(this);
        }

        private void OnMouseDown()
        {
            // WAŻNE: Jeśli kursor znajduje się nad oknem UI, nie reaguj na kliknięcie w kafelek z tyłu!
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            OnTileClicked();
        }
        
        private void OnMouseEnter()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            OnTileHoveredEvent?.Invoke(this);
        }

        private void OnMouseExit()
        {
            OnTileUnhoveredEvent?.Invoke();
        }
    }
}