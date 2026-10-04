using System;
using System.Collections.Generic;
using Core.GridPawns;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Input
{
    // One primary pointer owns a gesture. Read touch coordinates for both picking
    // and dragging, and produce only after a tap has finished without a drag.
    public class UserInput : MonoBehaviour
    {
        private Camera _cam;
        private GridPawn _activePawn;
        private Vector2 _pressPosition;
        private Vector2 _lastPosition;
        private bool _dragged;
        private bool _usingTouch;
        private int _touchId;
        private static bool _isInputOn = true;
        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

        public static event Action<GridPawn> OnGridPawnSingleTouched;
        // Preserve the existing presenter event contract; a single tap now produces.
        public static event Action OnGridPawnDoubleTouched;
        public static event Action OnGridPawnReleased;
        public static Vector2 PointerPosition { get; private set; }

        private void Awake() => _cam = Camera.main;

        private void Update()
        {
            var touch = Touchscreen.current?.primaryTouch;
            if (touch != null && touch.press.wasPressedThisFrame && _activePawn == null)
            {
                _usingTouch = true;
                _touchId = touch.touchId.ReadValue();
                Begin(touch.position.ReadValue());
            }
            else if (_activePawn == null && touch?.press.isPressed != true &&
                     Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                _usingTouch = false;
                Begin(Mouse.current.position.ReadValue());
            }

            if (_activePawn == null) return;
            if (!_isInputOn || !_activePawn.gameObject.activeInHierarchy)
            {
                End(false);
                return;
            }

            if (_usingTouch)
            {
                if (touch == null || touch.touchId.ReadValue() != _touchId)
                {
                    End(false);
                    return;
                }
                Move(touch.position.ReadValue());
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                    End(false);
                else if (touch.press.wasReleasedThisFrame || !touch.press.isPressed)
                    End(true);
            }
            else if (Mouse.current != null)
            {
                Move(Mouse.current.position.ReadValue());
                if (Mouse.current.leftButton.wasReleasedThisFrame || !Mouse.current.leftButton.isPressed)
                    End(true);
            }
        }

        private void Begin(Vector2 position)
        {
            PointerPosition = _lastPosition = _pressPosition = position;
            if (!_isInputOn || _cam == null || IsPointerOverUI(position)) return;
            var hit = Physics2D.Raycast(_cam.ScreenToWorldPoint(position), Vector2.zero);
            if (!hit || !hit.transform.TryGetComponent(out _activePawn)) return;
            _dragged = false;
            OnGridPawnSingleTouched?.Invoke(_activePawn);
            _activePawn.PawnEffect.SetFocus(true);
        }

        private void Move(Vector2 position)
        {
            PointerPosition = position;
            float threshold = Mathf.Max(10f, Screen.dpi > 0 ? Screen.dpi * 0.04f : 10f);
            if (!_dragged && Vector2.Distance(position, _pressPosition) >= threshold)
            {
                _dragged = true;
                _activePawn.PawnEffect.SetFocus(false); // Cancel any snap tween before dragging.
            }
            if (_dragged)
            {
                var delta = _cam.ScreenToWorldPoint(position) - _cam.ScreenToWorldPoint(_lastPosition);
                _activePawn.transform.position += new Vector3(delta.x, delta.y, 0f);
            }
            _lastPosition = position;
        }

        private void End(bool allowTap)
        {
            if (_activePawn == null) return;
            if (_activePawn.gameObject.activeInHierarchy)
            {
                if (!allowTap)
                    _activePawn.SetWorldPosition(Core.Helpers.GridPositionHelper.GetWorldPositionFromCoordinate(_activePawn.Coordinate));
                OnGridPawnReleased?.Invoke();
                if (allowTap && !_dragged && _isInputOn && !IsPointerOverUI(PointerPosition))
                    OnGridPawnDoubleTouched?.Invoke();
            }
            _activePawn = null;
        }

        private void OnDisable() => End(false);
        private void OnApplicationFocus(bool focused) { if (!focused) End(false); }
        private void OnApplicationPause(bool paused) { if (paused) End(false); }
        public static void SetInputState(bool isInputOn) => _isInputOn = isInputOn;

        private bool IsPointerOverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            _uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, _uiHits);
            return _uiHits.Count > 0;
        }
    }
}
