using System;
using System.Collections.Generic;
using System.Linq;
using Core.Factories.Interface;
using Core.GridEffects;
using Core.GridPawns;
using Core.Helpers;
using Core.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MVP.Models.Interface;
using MVP.Presenters.Handlers;
using UnityEngine;

namespace MVP.Presenters
{
    public class TaskPresenter
    {
        private const int MaxTaskCount = 2;
        private readonly IGridModel _gridModel;
        private readonly ITaskModel _taskModel;
        private readonly ITaskUIFactory _taskUIFactory;
        private readonly DisappearEffectHandler _disappearEffectHandler;
        private readonly GridPawnFactoryHandler _gridPawnFactoryHandler;

        private List<TaskUI> _activeTasks = new List<TaskUI>();
        private GridPawn[,] _grid;
        private readonly HashSet<int> _completingTasks = new HashSet<int>();

        public TaskPresenter(IGridModel gridModel, DisappearEffectHandler disappearEffectHandler,
            GridPawnFactoryHandler gridPawnFactoryHandler, ITaskModel taskModel,
            ITaskUIFactory taskUIFactory)
        {
            _gridModel = gridModel;
            _disappearEffectHandler = disappearEffectHandler;
            _gridPawnFactoryHandler = gridPawnFactoryHandler;
            _taskModel = taskModel;
            _taskUIFactory = taskUIFactory;
        }

        public async UniTask InitializeTasks()
        {
            _grid = _gridModel.Grid;
            for (int i = 0; i < MaxTaskCount; i++)
            {
                LoadNextTask().Forget();
            }

            UpdateTasks();

            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), DelayType.DeltaTime);
        }

        public void UpdateGridAfterEditor()
        {
            _grid = _gridModel.Grid;
        }

        public async UniTask CompleteTask(int taskID, List<Appliance> appliancesToDestroy)
        {
            var taskToComplete = _activeTasks.FirstOrDefault(task => task.TaskID == taskID);
            if (taskToComplete == null)
            {
                Debug.LogWarning("Completed task cannot find the active tasks!");
                return;
            }

            if (_completingTasks.Contains(taskID)) return;
            // Revalidate immediately: another order may have consumed a shared item.
            var goals = taskToComplete.ActiveGoals.Select(ui => ui.Goal).ToList();
            var currentMatches = TaskRequirementMatcher.Match(_grid, goals);
            if (currentMatches == null)
            {
                UpdateTasks();
                return;
            }
            _completingTasks.Add(taskID);
            taskToComplete.CanvasGroup.interactable = false;
            DestroyAppliances(currentMatches);
            UpdateTasks();
            await AnimateCompletedTask(taskToComplete);
            _activeTasks.Remove(taskToComplete);
            _taskUIFactory.DestroyObj(taskToComplete);
            _taskModel.CompleteTask(taskID);
            _completingTasks.Remove(taskID);
            LoadNextTask().Forget();
            UpdateTasks();
        }

        private async UniTask AnimateCompletedTask(TaskUI completedTask, float animDuration = 0.3f)
        {
            var cg = completedTask.CanvasGroup;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            DOTween.To(() => cg.alpha, x => cg.alpha = x, 0, animDuration);
            await UniTask.Delay(TimeSpan.FromSeconds(animDuration), DelayType.DeltaTime);
        }

        private async UniTask AnimateNewTask(TaskUI newTask, float animDuration = 0.3f)
        {
            var cg = newTask.CanvasGroup;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            DOTween.To(() => cg.alpha, x => cg.alpha = x, 1, animDuration);
            await UniTask.Delay(TimeSpan.FromSeconds(animDuration), DelayType.DeltaTime);
        }

        private void HandleApplianceDestruction(GridPawn pawn)
        {
            _disappearEffectHandler.PlayDisappearEffect(pawn.transform.position, ColorType.Green).Forget();
            _gridModel.UpdateGridPawn(pawn, true);
            _gridPawnFactoryHandler.DestroyPawn(pawn);
            pawn.PawnEffect.SetFocus(false);
        }

        private void DestroyAppliances(List<Appliance> appliancesToDestroy)
        {
            foreach (var appliance in appliancesToDestroy)
            {
                HandleApplianceDestruction(appliance);
            }
        }

        private async UniTask LoadNextTask()
        {
            var taskInfo = _taskModel.GetNextTask();
            if (taskInfo == null) return;

            var newTaskUI = _taskUIFactory.CreateObj();

            newTaskUI.TaskID = taskInfo.TaskID;
            newTaskUI.CharImage.texture = taskInfo.CharTexture;

            foreach (var goal in taskInfo.Goals)
            {
                var sprite = _gridPawnFactoryHandler.GetSprite(goal.ApplianceType, goal.Level);
                if (sprite != null)
                {
                    newTaskUI.SetGoalUI(goal, sprite);
                }
                else
                {
                    Debug.LogWarning($"Missing appliance data for {goal.ApplianceType} at level {goal.Level}");
                }
            }

            newTaskUI.SubscribeDoneButton(this);
            _activeTasks.Add(newTaskUI);
            newTaskUI.CanvasGroup.alpha = 0f;
            await AnimateNewTask(newTaskUI);
        }

        public void UpdateTasks()
        {
            foreach (var pawn in _grid)
                if (pawn is Appliance appliance) appliance.PawnEffect.SetGlowing(false);

            foreach (var taskUI in _activeTasks)
            {
                if (_completingTasks.Contains(taskUI.TaskID)) continue;
                var reserved = new HashSet<GridPawn>();
                foreach (var goalUI in taskUI.ActiveGoals)
                {
                    GridPawn match = null;
                    foreach (var pawn in _grid)
                    {
                        if (pawn is Appliance && pawn.Type.Equals(goalUI.Goal.ApplianceType) &&
                            pawn.Level == goalUI.Goal.Level && reserved.Add(pawn))
                        {
                            match = pawn;
                            break;
                        }
                    }
                    taskUI.MatchGoal(goalUI, match);
                }
            }
        }
    }
}
