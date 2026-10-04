using UnityEngine;

namespace UIExtensions
{
    public static class TabletLayoutMath
    {
        public static Rect BoardArea(Rect safeArea, float width, float height)
        {
            return new Rect((safeArea.x + safeArea.width * 0.015f) / width,
                (safeArea.y + safeArea.height * 0.16f) / height,
                safeArea.width * 0.97f / width, safeArea.height * 0.54f / height);
        }

        public static float CameraSize(Vector2 boardSize, Rect viewport, float aspect)
        {
            return Mathf.Max(boardSize.x / (viewport.width * aspect), boardSize.y / viewport.height) * 0.515f;
        }
    }
}
