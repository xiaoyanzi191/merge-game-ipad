using Core.GridPawns.Data;
using Core.GridPawns.Effect;
using Core.GridPawns.Interface;
using Core.Helpers;
using UnityEngine;

namespace Core.GridPawns
{
    public abstract class GridPawn : MonoBehaviour, IGridPawn, IType
    {
        [field: SerializeField] public BoxCollider2D BoxCollider { get; private set; }
        [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
        [field: SerializeField] public GridPawnEffect PawnEffect { get; private set; }
        [field: SerializeField] public Vector2Int Coordinate { get; set; }


        public abstract System.Enum Type { get; protected set; } // Enforced by derived classes to follow IType

        public int MaxLevel { get; set; }

        public int Level
        {
            get => _level;
            private set
            {
                _level = value;
                PawnEffect.SetLastLevel(MaxLevel == _level);
            }
        }

        private int _level;

        private void OnEnable()
        {
            // A recycled pawn must not retain the collider state of an interrupted move.
            if (BoxCollider != null) BoxCollider.enabled = true;
        }

        public void SetWorldPosition(Vector3 worldPos, bool isAnimOn = false, float animTime = 0.3f)
        {
            // Apply the position with or without animation
            if (isAnimOn)
            {
                int sortingOrder = SpriteRenderer.sortingOrder;

                SetSortingOrder(1000, "Additions");
                //BoxCollider.enabled = false;
                PawnEffect.Shift(worldPos, () =>
                {
                    BoxCollider.enabled = true;
                    SetSortingOrder(sortingOrder, "Pawns");
                }, animTime);
            }
            else
            {
                transform.position = worldPos;
            }
        }

        public void SetWorldPosition(Vector2 longestCell, Transform gridTopLeftTr,
            Vector2Int? coordinateOverride = null, bool isAnimOn = false, float animTime = 0.3f)
        {
            // Use the provided override coordinate or default to the current coordinate
            var targetCoordinate = coordinateOverride ?? Coordinate;
            // Calculate the world position
            var position = CalculateWorldPosition(longestCell, gridTopLeftTr, targetCoordinate);

            // Apply the position with or without animation
            if (isAnimOn)
            {
                int sortingOrder = SpriteRenderer.sortingOrder;

                SetSortingOrder(1000, "Additions");
                BoxCollider.enabled = false;
                PawnEffect.Shift(position, () =>
                {
                    BoxCollider.enabled = true;
                    SetSortingOrder(sortingOrder, "Pawns");
                }, animTime);
            }
            else
            {
                transform.position = position;
            }
        }

        // Helper Method for Position Calculation
        private Vector3 CalculateWorldPosition(Vector2 longestCell, Transform gridTopLeftTr,
            Vector2Int targetCoordinate)
        {
            var gridTr = gridTopLeftTr.parent;
            var scaleNormalizing = gridTr.localScale.x;

            return GridPositionHelper.CalculateItemWorldPosition(gridTopLeftTr.position, longestCell, targetCoordinate,
                scaleNormalizing);
        }


        // SetAttributes, leveraging IType and polymorphism
        public void SetAttributes(Vector2Int newCoord, System.Enum type, int level, int maxLevel)
        {
            Coordinate = newCoord;
            Type = type;
            MaxLevel = maxLevel;
            Level = level;
            name = ToString();
            SetSortingOrder(-newCoord.y, "Pawns");
        }

        public void SetSortingOrder(int order, string layerName)
        {
            SpriteRenderer.sortingLayerName = layerName;
            SpriteRenderer.sortingOrder = order;
        }

        public virtual void ApplyData(GridPawnLevelDataSO levelData)
        {
            var gridObjectWidthHeight = levelData.GridPawnWidthHeight;
            SpriteRenderer.size = new Vector2(gridObjectWidthHeight.x, gridObjectWidthHeight.y);
            BoxCollider.size = new Vector2(gridObjectWidthHeight.x, gridObjectWidthHeight.y);
        }

        public abstract override string ToString();
    }
}