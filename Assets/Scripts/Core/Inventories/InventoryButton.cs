using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Inventories
{
    public class InventoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [field: SerializeField] public Button Button { get; private set; }
        public bool IsEntered { get; set; }

        public bool ContainsPointer(Vector2 position)
        {
            var canvas = GetComponentInParent<Canvas>();
            var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, position, camera);
        }

        private void OnDisable() => IsEntered = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsEntered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsEntered = false;
        }
    }
}