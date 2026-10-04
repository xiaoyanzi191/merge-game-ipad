using System.Collections.Generic;
using System.Linq;
using Core.GridPawns;
using DG.Tweening;
using MVP.Presenters;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Tasks
{
    public class TaskUI : MonoBehaviour
    {
        public int TaskID { get; set; }
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        [field: SerializeField] public RawImage CharImage { get; set; }
        [field: SerializeField] public Button DoneButton { get; private set; }
        [field: SerializeField] public List<GoalUI> AllGoalUIs { get; private set; }

        private int _goalUIIndex;
        private Dictionary<GoalUI, Appliance> _matchedAppliances; // Track matched pawns

        public List<GoalUI> ActiveGoals { get; private set; }

        private void OnEnable()
        {
            _goalUIIndex = 0;
            DoneButton.transform.localScale = Vector3.zero;
            DoneButton.interactable = false;

            _matchedAppliances = new Dictionary<GoalUI, Appliance>();
            ActiveGoals = new List<GoalUI>();

            foreach (var goalUI in AllGoalUIs)
            {
                goalUI.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            foreach (var goalUI in AllGoalUIs)
            {
                goalUI.gameObject.SetActive(false);
            }

            DoneButton.onClick.RemoveAllListeners();
        }

        public void SubscribeDoneButton(TaskPresenter taskPresenter)
        {
            DoneButton.onClick.AddListener(async () =>
                await taskPresenter.CompleteTask(TaskID, _matchedAppliances.Values.ToList()));
        }

        public void ClearDict()
        {
            _matchedAppliances.Clear();
        }

        public void SetGoalUI(Goal goal, Sprite sprite)
        {
            AllGoalUIs[_goalUIIndex].gameObject.SetActive(true);
            var newGoal = new Goal
            {
                ApplianceType = goal.ApplianceType,
                Level = goal.Level
            };
            AllGoalUIs[_goalUIIndex].Goal = newGoal;
            AllGoalUIs[_goalUIIndex].GoalImage.sprite = sprite;

            ActiveGoals.Add(AllGoalUIs[_goalUIIndex++]);
        }

        public void MatchGoal(GoalUI goalUI, GridPawn pawn)
        {
            if (pawn is Appliance appliance)
            {
                _matchedAppliances[goalUI] = appliance; // Store valid match
                goalUI.SetCompletion(true); // Mark goal as completed
                appliance.PawnEffect.SetGlowing(true);
            }
            else
            {
                _matchedAppliances.Remove(goalUI); // Remove invalid match
                goalUI.SetCompletion(false); // Reset completion if no match is found
            }

            CheckAllGoalsCompleted(); // Recalculate if "Done" button should be visible
        }

        private void CheckAllGoalsCompleted()
        {
            if (ActiveGoals.Count > 0 && _matchedAppliances.Count == ActiveGoals.Count)
            {
                SetDoneButton(true);
            }
            else
            {
                SetDoneButton(false);
            }
        }

        private void SetDoneButton(bool isDone)
        {
            DoneButton.interactable = isDone;
            DoneButton.transform.DOKill();

            if (isDone)
            {
                DoneButton.transform.DOScale(1f, 0.3f);
            }
            else
            {
                DoneButton.transform.DOScale(0f, 0.3f);
            }
        }
    }
}