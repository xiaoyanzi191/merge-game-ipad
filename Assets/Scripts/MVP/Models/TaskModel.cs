using System.Collections.Generic;
using System.Linq;
using Core.Tasks;
using Core.Helpers;
using MVP.Models.Interface;
using UnityEngine;

namespace MVP.Models
{
    public class TaskModel : ITaskModel
    {
        private Queue<TaskSO> _taskQueue; // Queue to store available tasks
        private List<int> _completedTaskIDs; // Store IDs of completed tasks

        public TaskModel()
        {
            LoadTasks();
        }

        private void LoadTasks()
        {
            // Load all available tasks from Resources
            var allTasks = Resources.LoadAll<TaskSO>("Tasks").ToList();

            // Load completed task IDs from PlayerPrefs
            _completedTaskIDs = LoadCompletedTasks();

            // Filter out completed tasks and order by TaskID
            var remainingTasks = allTasks
                .Where(task => !_completedTaskIDs.Contains(task.TaskID))
                .OrderBy(task => task.TaskID); // Sort by TaskID
            // Initialize queue with remaining tasks
            _taskQueue = new Queue<TaskSO>(remainingTasks);

            Debug.Log($"Loaded Tasks: {_taskQueue.Count}, Completed Tasks: {_completedTaskIDs.Count}");
        }

        public void CompleteTask(int taskID)
        {
            if (!_completedTaskIDs.Contains(taskID))
            {
                _completedTaskIDs.Add(taskID);
                SaveCompletedTasks();

                Debug.Log($"Task {taskID} completed! Remaining Tasks: {_taskQueue.Count}");
            }
        }

        public TaskSO GetNextTask()
        {
            if (_taskQueue.Count > 0)
            {
                var task = _taskQueue.Dequeue();
                return task;
            }

            return null;
        }

        private void SaveCompletedTasks()
        {
            string json = string.Join(",", _completedTaskIDs.Select(id => id.ToString()).ToArray());
            BrowserSaveStorage.WriteCompletedTasks(json);
        }

        private List<int> LoadCompletedTasks()
        {
            string json = BrowserSaveStorage.ReadCompletedTasks();
            if (json == null) return new List<int>();
            return json.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
        }
    }
}
