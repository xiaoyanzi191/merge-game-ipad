using System;
using Core.GridPawns;
using Core.GridSerialization;
using Core.Helpers;
using Cysharp.Threading.Tasks;
using MVP.Models.Interface;
using MVP.Views.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MVP.Presenters
{
    public class GridPresenter
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;

        public GridPresenter(IGridModel gridModel, IGridView gridView)
        {
            _gridModel = gridModel;
            _gridView = gridView;

            _gridModel.OnGridCoordinateToWorldPosCalculated += GridCoordinateToWorldPosCalculated;
            _gridModel.OnGridPawnInitialized += GridPawnInitialized;
            _gridModel.OnGridPawnUpdated += GridPawnUpdated;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            Dispose();
        }

        private void Dispose()
        {
            _gridModel.OnGridCoordinateToWorldPosCalculated -= GridCoordinateToWorldPosCalculated;
            _gridModel.OnGridPawnInitialized -= GridPawnInitialized;
            _gridModel.OnGridPawnUpdated -= GridPawnUpdated;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public async UniTask LoadGrid()
        {
            // TODO: Addressable logic next step
            var gridInfo = _gridModel.GetGridInfo();
            Vector2Int gridSize = gridInfo?.GridSize ?? new Vector2Int(_gridModel.ColumnCount, _gridModel.RowCount);

            GridPositionHelper.ClearPositions();
            _gridView.CalculateGridSize(gridSize);
            _gridModel.LoadGrid(gridInfo);

            _gridView.Scale(gridSize);

            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), DelayType.DeltaTime);
        }


        public void LoadFromGridEditor(GridInfo gridInfo)
        {
            GridPositionHelper.ClearPositions();
            _gridView.CalculateGridSize(gridInfo.GridSize);
            _gridModel.LoadGrid(gridInfo);
            _gridView.Scale(gridInfo.GridSize);
        }

        private void GridCoordinateToWorldPosCalculated(Vector2Int coord)
        {
            var gridTr = _gridView.GridTopLeftTr.parent;
            var scaleNormalizing = gridTr.localScale.x;
            GridPositionHelper.CalculateItemWorldPosition(_gridView.GridTopLeftTr.position, _gridView.CellSize, coord,
                scaleNormalizing);
        }

        private void GridPawnInitialized(GridPawn obj)
        {
            obj.SetWorldPosition(_gridView.CellSize, _gridView.GridTopLeftTr);
        }

        private void GridPawnUpdated(GridPawn obj, Vector2Int? newCoord, bool isAnimOn, float animTime)
        {
            obj.SetWorldPosition(_gridView.CellSize, _gridView.GridTopLeftTr, newCoord);
            obj.SetWorldPosition(_gridView.CellSize, _gridView.GridTopLeftTr, null, isAnimOn, animTime);
        }
    }
}