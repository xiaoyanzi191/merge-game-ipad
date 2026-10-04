using MVP.Views.Interface;
using UnityEngine;
using UIExtensions;

namespace MVP.Views
{
    public class GridView : MonoBehaviour, IGridView
    {
        [field: SerializeField] public SpriteRenderer GridSprite { get; private set; }
        [field: SerializeField] public Transform GridTopLeftTr { get; private set; }
        [field: SerializeField] public Vector2 CellSize { get; private set; }
        [field: SerializeField] public Vector2 GridTopLeftMargin { get; private set; }
        [field: SerializeField] public Vector2 GridPadding { get; private set; }

        [field: SerializeField] public Camera Cam { get; private set; }
        private bool _ready;
        private Rect _safeArea;
        private Vector2Int _screenSize;

        public void CalculateGridSize(Vector2Int gridSize)
        {
            var cellHeight = CellSize.y;
            var cellWidth = CellSize.x;

            // Calculate scaled padding based on grid size
            var paddingFactorX = 1f / gridSize.y;
            var paddingFactorY = 1f / gridSize.x;

            var basePadding = GridPadding;
            var scaledPadding = new Vector2(basePadding.x * paddingFactorX, basePadding.y * paddingFactorY);
            GridSprite.size = new Vector2((cellWidth + scaledPadding.x) * gridSize.y,
                (cellHeight + scaledPadding.y) * gridSize.x);
            UpdateGridTopLeftTr();
        }

        public void Scale(Vector2Int gridSize)
        {
            _ready = true;
            FrameBoard();
        }

        private void LateUpdate()
        {
            if (_ready && (_safeArea != Screen.safeArea || _screenSize.x != Screen.width || _screenSize.y != Screen.height))
                FrameBoard();
        }

        private void FrameBoard()
        {
            _safeArea = Screen.safeArea;
            _screenSize = new Vector2Int(Screen.width, Screen.height);
            var viewport = TabletLayoutMath.BoardArea(_safeArea, Screen.width, Screen.height);
            var bounds = GridSprite.bounds;
            Cam.orthographicSize = TabletLayoutMath.CameraSize(bounds.size, viewport, Cam.aspect);
            var worldHeight = Cam.orthographicSize * 2f;
            var position = Cam.transform.position;
            position.x = bounds.center.x - (viewport.center.x - 0.5f) * worldHeight * Cam.aspect;
            position.y = bounds.center.y - (viewport.center.y - 0.5f) * worldHeight;
            Cam.transform.position = position;
        }

        private void UpdateGridTopLeftTr()
        {
            var bounds = GridSprite.bounds;
            GridTopLeftTr.position = new Vector2(bounds.min.x, bounds.max.y) + GridTopLeftMargin;
        }
    }
}