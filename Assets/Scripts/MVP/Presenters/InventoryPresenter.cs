using System.Linq;
using Core.Factories.Interface;
using Core.GridPawns;
using Core.Helpers;
using Core.Inventories;
using Input;
using MVP.Models.Interface;
using MVP.Presenters.Handlers;
using MVP.Views.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MVP.Presenters
{
    public class InventoryPresenter
    {
        private readonly IInventoryModel _inventoryModel;
        private readonly IInventoryView _inventoryView;
        private readonly IInventoryPawnUIFactory _inventoryPawnUIFactory;
        private readonly IGridModel _gridModel;
        private readonly GridPawnFactoryHandler _gridPawnFactoryHandler;
        private readonly TaskPresenter _taskPresenter;

        private GridPawn _activePawn;

        public InventoryPresenter(IInventoryModel inventoryModel, IInventoryView inventoryView,
            IInventoryPawnUIFactory inventoryPawnUIFactory,
            IGridModel gridModel, GridPawnFactoryHandler gridPawnFactoryHandler, TaskPresenter taskPresenter)
        {
            _inventoryModel = inventoryModel;
            _inventoryView = inventoryView;
            _inventoryPawnUIFactory = inventoryPawnUIFactory;
            _gridModel = gridModel;
            _gridPawnFactoryHandler = gridPawnFactoryHandler;
            _taskPresenter = taskPresenter;

            UserInput.OnGridPawnSingleTouched += OnTouched;
            UserInput.OnGridPawnReleased += OnReleased;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            _inventoryView.SubscribeInventoryButton(this);
            foreach (var inventoryPawn in _inventoryModel.Pawns)
            {
                AddPawnToInventoryAfterLoad(inventoryPawn);
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            Dispose();
        }

        private void Dispose()
        {
            // Unsubscribe from static and instance events
            UserInput.OnGridPawnSingleTouched -= OnTouched;
            UserInput.OnGridPawnReleased -= OnReleased;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void OnPawnRequestedFromInventory(InventoryPawn inventoryPawn)
        {
            var matchingUI = FindMatchingInventoryUI(inventoryPawn);
            if (matchingUI == null)
            {
                Debug.LogWarning($"No InventoryPawnUI found with ID: {inventoryPawn.ID}");
                return;
            }

            var randomCoordinate = GridPositionHelper.FindRandomEmptyCoordinate(_gridModel.Grid);
            if (randomCoordinate == null)
            {
                HandleNoSpaceInGrid(matchingUI);
                return;
            }

            ProcessPawnRequest(matchingUI, inventoryPawn, randomCoordinate.Value);
        }

        // Find the matching UI element for the requested pawn
        private InventoryPawnUI FindMatchingInventoryUI(InventoryPawn inventoryPawn)
        {
            return _inventoryView.InventoryPawnUIs
                .FirstOrDefault(ui => ui.InventoryPawn.ID == inventoryPawn.ID);
        }

        // Handle case where there is no space in the grid
        private void HandleNoSpaceInGrid(InventoryPawnUI matchingUI)
        {
            matchingUI.Shake(); // Indicate no space available
        }

        // Process moving the pawn from inventory to the grid
        private void ProcessPawnRequest(InventoryPawnUI matchingUI, InventoryPawn inventoryPawn, Vector2Int coordinate)
        {
            PlayPawnRemoveFromInventoryEffect();

            // Remove pawn from inventory
            _inventoryPawnUIFactory.DestroyObj(matchingUI);
            _inventoryView.InventoryPawnUIs.Remove(matchingUI);
            _inventoryModel.RemovePawn(inventoryPawn);

            // Create and place the pawn on the grid
            var newPawn = _gridPawnFactoryHandler.CreateGridPawn(inventoryPawn.Type, inventoryPawn.Level, coordinate);
            _gridModel.UpdateGridPawn(newPawn, false, null, true, 0.2f);

            _taskPresenter.UpdateTasks();
        }

        private void PlayPawnRemoveFromInventoryEffect()
        {
            // TODO: Implement particle effect here
        }


        public void OnInventoryRequested()
        {
            _inventoryView.OpenInventoryPanel();
        }

        private void OnTouched(GridPawn touchedGridPawn)
        {
            _activePawn = touchedGridPawn;
        }

        private void OnReleased()
        {
            if (_activePawn == null || !_activePawn.gameObject.activeInHierarchy ||
                _gridModel.Grid[_activePawn.Coordinate.x, _activePawn.Coordinate.y] != _activePawn ||
                !_inventoryView.InventoryButton.ContainsPointer(UserInput.PointerPosition)) return;

            PlayReleaseEffects();
            AddPawnToInventory();
            RemovePawnFromGrid();

            _taskPresenter.UpdateTasks();
            _activePawn = null;
        }

        private void PlayReleaseEffects()
        {
            _inventoryView.PlayInventoryButtonParticle();
        }

        //  Create and add pawn to inventory
        private void AddPawnToInventory()
        {
            var newInventoryPawnUI = _inventoryPawnUIFactory.CreateObj();
            newInventoryPawnUI.FillData(_activePawn);
            newInventoryPawnUI.SubscribeToInventoryPawnUIClick(this);
            _inventoryView.InventoryPawnUIs.Add(newInventoryPawnUI);

            _inventoryModel.AddPawn(newInventoryPawnUI.InventoryPawn); //  Save to inventory
        }

        private void AddPawnToInventoryAfterLoad(InventoryPawn inventoryPawn)
        {
            var newInventoryPawnUI = _inventoryPawnUIFactory.CreateObj();
            newInventoryPawnUI.InventoryPawn = inventoryPawn;

            newInventoryPawnUI.SetPawnUISprite(
                _gridPawnFactoryHandler.GetSprite(inventoryPawn.Type, inventoryPawn.Level));

            newInventoryPawnUI.SubscribeToInventoryPawnUIClick(this);
            _inventoryView.InventoryPawnUIs.Add(newInventoryPawnUI);
        }

        //  Remove the pawn from the grid
        private void RemovePawnFromGrid()
        {
            _gridPawnFactoryHandler.DestroyPawn(_activePawn);
            _gridModel.UpdateGridPawn(_activePawn, true);
            _activePawn.PawnEffect.SetFocus(false);
        }
    }
}