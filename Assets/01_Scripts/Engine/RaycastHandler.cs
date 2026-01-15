using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ThreeMatch
{
    public class RaycastHandler : MonoBehaviour
    {
        private Camera _cam;
        private LayerMask _rayLayerMask;
        private bool _ignoreWhenPointerOverUI = true;

        private bool _raycastEnabled = false;
        private bool _isDragging = false;
        private IRaycastable _current = null;

        private InputAction _pointAction;
        private InputAction _clickAction;

        private Vector2 _prevScreenPos;
        private Vector2 _currScreenPos;

        public Vector2 DragDelta { get; private set; }
        private void Awake()
        {
            _rayLayerMask = 1 << LayerMask.NameToLayer("Raycastable");
            UpdateCurrentMainCamera();

            _pointAction = InputSystem.actions.FindAction("Point");
            _clickAction = InputSystem.actions.FindAction("Click");
        }

        private void OnDestroy()
        {
            _pointAction?.Dispose();
            _clickAction?.Dispose();
            _current = null;
        }

        private void Update()
        {
            if (!_raycastEnabled) return;
            Vector2 screenPos = _pointAction.ReadValue<Vector2>();

            if (_clickAction.WasPressedThisFrame())
            {
                HandlePointerDown(screenPos);
            }
            else if (_clickAction.IsPressed())
            {
                HandlePointerDrag(screenPos);
            }
            else if (_clickAction.WasReleasedThisFrame())
            {
                HandlePointerUp(screenPos);
            }
        }

        private void HandlePointerDown(Vector2 screenPos)
        {
            if (_ignoreWhenPointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            _current = RaycastTopRaycastable(screenPos);
            _isDragging = false;

            _prevScreenPos = screenPos;
            _currScreenPos = screenPos;
            DragDelta = Vector2.zero;

            _current?.OnPointerDown();
        }

        private void HandlePointerDrag(Vector2 screenPos)
        {
            if (_current == null) return;

            _currScreenPos = screenPos;
            DragDelta = _currScreenPos - _prevScreenPos;

            if (!_isDragging)
            {
                _isDragging = true;
                _current.OnBeginDrag();
            }

            _current.OnDrag(DragDelta);

            _prevScreenPos = _currScreenPos;
        }

        private void HandlePointerUp(Vector2 screenPos)
        {
            if (_current == null) return;

            _currScreenPos = screenPos;
            DragDelta = _currScreenPos - _prevScreenPos;

            if (_isDragging)
                _current.OnEndDrag();

            _current.OnPointerUp();
            
            var topRaycastable = RaycastTopRaycastable(screenPos);
            
            if (_current == topRaycastable)
                _current.OnPointerClick();

            _current = null;
            _isDragging = false;

            DragDelta = Vector2.zero;
        }

        private IRaycastable RaycastTopRaycastable(Vector2 screenPos)
        {
            var world = _cam.ScreenToWorldPoint(screenPos);

            var hits = Physics2D.RaycastAll(world, Vector2.zero, _cam.farClipPlane, _rayLayerMask);
            if (hits == null || hits.Length == 0) return null;

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            IRaycastable raycast = null;
            foreach (var hit in hits)
            {
                var raycastable = FindRaycastable(hit.collider);
                if (raycastable != null)
                {
                    if (raycast == null || raycastable.RaycastOrder > raycast.RaycastOrder)
                        raycast = raycastable;
                }
            }

            return raycast;
        }

        private static IRaycastable FindRaycastable(Collider2D col)
        {
            var mbs = col.GetComponentsInParent<MonoBehaviour>(includeInactive: false);
            foreach (var mb in mbs)
            {
                if (mb is IRaycastable r)
                    return r;
            }
            return null;
        }

        public void UpdateCurrentMainCamera() => _cam = Camera.main;
        public void RaycastEnabled(bool enabled) => _raycastEnabled = enabled;
    }
}

