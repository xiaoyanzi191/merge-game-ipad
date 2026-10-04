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
        private Mouse _gestureMouse;
        private Touchscreen _gestureTouchscreen;
        private InputAction _pressAction;
        private InputAction _positionAction;
        private static bool _isInputOn = true;
        private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

        public static event Action<GridPawn> OnGridPawnSingleTouched;
        // Preserve the existing presenter event contract; a single tap now produces.
        public static event Action OnGridPawnDoubleTouched;
        public static event Action OnGridPawnReleased;
        public static Vector2 PointerPosition { get; private set; }

        private void Awake()
        {
            // Loading and merge scenes coexist during transitions; bind to this
            // scene's camera instead of whichever MainCamera is found first.
            foreach (var camera in FindObjectsOfType<Camera>())
                if (camera.gameObject.scene == gameObject.scene && camera.CompareTag("MainCamera"))
                {
                    _cam = camera;
                    break;
                }

            // Actions preserve press/release edges even when both arrive before Update.
            _pressAction = new InputAction("MergePress", InputActionType.Button);
            _pressAction.AddBinding("<Mouse>/leftButton");
            _pressAction.AddBinding("<Touchscreen>/primaryTouch/press");
            _pressAction.performed += OnPress;
            _pressAction.canceled += OnRelease;
            _positionAction = new InputAction("MergePosition", InputActionType.PassThrough);
            _positionAction.AddBinding("<Mouse>/position");
            _positionAction.AddBinding("<Touchscreen>/primaryTouch/position");
            _positionAction.performed += OnPosition;
        }

        private void OnEnable()
        {
            _positionAction?.Enable();
            _pressAction?.Enable();
        }

        private void OnPress(InputAction.CallbackContext context)
        {
            if (_activePawn != null) return;
            _gestureTouchscreen = context.control.device as Touchscreen;
            _gestureMouse = context.control.device as Mouse;
            _usingTouch = _gestureTouchscreen != null;
            if (_usingTouch)
            {
                var touch = _gestureTouchscreen.primaryTouch;
                _touchId = touch.touchId.ReadValue();
                Begin(touch.position.ReadValue());
            }
            else if (_gestureMouse != null) Begin(_gestureMouse.position.ReadValue());
        }

        private void OnPosition(InputAction.CallbackContext context)
        {
            if (_activePawn == null) return;
            if (context.control.device == (_usingTouch ? (InputDevice)_gestureTouchscreen : _gestureMouse))
                Move(context.ReadValue<Vector2>());
        }

        private void OnRelease(InputAction.CallbackContext context)
        {
            if (_activePawn == null) return;
            if (_usingTouch)
            {
                var touch = _gestureTouchscreen?.primaryTouch;
                if (touch == null || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                { End(false); return; }
                if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.None)
                    Move(touch.position.ReadValue());
            }
            else if (_gestureMouse != null) Move(_gestureMouse.position.ReadValue());
            End(true);
        }

        private void Update()
        {
            if (_activePawn == null) return;
            if (!_isInputOn || !_activePawn.gameObject.activeInHierarchy)
            {
                End(false);
                return;
            }

            if (_usingTouch)
            {
                var touch = _gestureTouchscreen?.primaryTouch;
                if (touch == null || touch.touchId.ReadValue() != _touchId)
                {
                    End(false);
                    return;
                }
                Move(touch.position.ReadValue());
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                    End(false);
                else if (!touch.press.isPressed)
                    End(true);
            }
            else if (_gestureMouse != null)
            {
                Move(_gestureMouse.position.ReadValue());
                if (!_gestureMouse.leftButton.isPressed)
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
            if (_activePawn == null || _cam == null) return;
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

        private void OnDisable()
        {
            End(false);
            _pressAction?.Disable();
            _positionAction?.Disable();
        }
        private void OnDestroy()
        {
            _pressAction?.Dispose();
            _positionAction?.Dispose();
        }
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
