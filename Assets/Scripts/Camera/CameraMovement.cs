using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace IdleBuilder.World
{
    public class CameraMovement : MonoBehaviour
    {
        [Header("Drag Settings")]
        [SerializeField] private float dragSpeed = 1f;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 25f;

        private Camera _cam;
        private Vector3 _dragOrigin;
        private bool _isDragging;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            // Sprawdzamy czy mysz jest dostępna
            if (Mouse.current == null) return;

            // Ignorujemy sterowanie, gdy kursor znajduje się nad interfejsem UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            HandleDrag();
            HandleZoom();
        }

        private void HandleDrag()
        {
            // Przesuwanie Prawym Przyciskiem Myszy (Right Button)
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _isDragging = true;
                _dragOrigin = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }

            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }

            if (_isDragging && Mouse.current.rightButton.isPressed)
            {
                Vector3 currentMousePos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector3 difference = _dragOrigin - currentMousePos;
                transform.position += difference * dragSpeed;
            }
        }

        private void HandleZoom()
        {
            // Pobieranie wartości kółka myszy z Nowego Input Systemu
            float scrollY = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scrollY) > 0.01f)
            {
                // W nowym Input Systemie skok scrolla wynosi zazwyczaj +/- 120
                float zoomDelta = (scrollY / 120f) * zoomSpeed;
                float newSize = _cam.orthographicSize - zoomDelta;
                _cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }
    }
}