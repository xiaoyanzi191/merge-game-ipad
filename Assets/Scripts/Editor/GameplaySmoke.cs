using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;
using Core.Factories;
using Core.GridPawns;
using Core.GridPawns.Enum;
using Core.GridSerialization;
using Core.Helpers;
using Core.Tasks;
using DI.Contexts;
using MVP.Models.Interface;
using MVP.Presenters;
using MVP.Presenters.Handlers;
using MVP.Views;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LocalMerge.Editor
{
    // Real Unity Play Mode smoke checks. Run only in a dedicated batch build
    // workspace; the unique product name isolates saves from the actual game.
    [InitializeOnLoad]
    public static class GameplaySmoke
    {
        private const string Key = "LocalMerge.Smoke.";
        static GameplaySmoke()
        {
            if (SessionState.GetBool(Key + "Active", false)) Register();
        }

        public static void Run()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Run smoke tests in a dedicated batchmode workspace.");
            Directory.CreateDirectory("Builds");
            if (File.Exists("Builds/smoke-result.json")) File.Delete("Builds/smoke-result.json");
            SessionState.SetString(Key + "Company", PlayerSettings.companyName);
            SessionState.SetString(Key + "Product", PlayerSettings.productName);
            PlayerSettings.companyName = "LocalMergeSmoke";
            PlayerSettings.productName = "Isolated-" + Guid.NewGuid().ToString("N");
            SessionState.SetBool(Key + "Active", true);
            SessionState.SetInt(Key + "Stage", 1);
            SessionState.SetFloat(Key + "Started", (float)EditorApplication.timeSinceStartup);
            SessionState.SetString(Key + "Error", "");
            EditorSceneManager.OpenScene("Assets/Scenes/LoadScene.unity");
            // A fixed portrait Game View is needed for deterministic editor UI bounds.
            ConfigurePortraitGameView();
            Register();
            EditorApplication.EnterPlaymode();
        }

        private static void ConfigurePortraitGameView()
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance").GetValue(null);
            var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { assembly.GetType("UnityEditor.GameViewSizes").GetProperty("currentGroupType").GetValue(sizes) });
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var kindType = assembly.GetType("UnityEditor.GameViewSizeType");
            var size = Activator.CreateInstance(sizeType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { Enum.Parse(kindType, "FixedResolution"), (object)768, 1024, "Merge smoke portrait" }, null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            int index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null) - 1;
            var viewType = assembly.GetType("UnityEditor.GameView");
            var view = EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, index);
        }

        private static void Register()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Application.logMessageReceived -= Log;
            Application.logMessageReceived += Log;
        }

        private static void Log(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                SessionState.SetString(Key + "Error", message + "\n" + stack);
        }

        private static void Require(bool condition, string description)
        {
            if (!condition) throw new Exception(description);
            Debug.Log("SMOKE PASS: " + description);
        }

        private static object Invoke(object target, string method, params object[] args)
        {
            return target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        }

        private static void VerifyRapidPointerTaps(IGridModel model, GridPawnFactoryHandler handler,
            GridView views, Producer producer)
        {
            var input = UnityEngine.Object.FindObjectOfType<Input.UserInput>();
            var press = (InputAction)typeof(Input.UserInput).GetField("_pressAction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(input);
            var position = (InputAction)typeof(Input.UserInput).GetField("_positionAction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(input);
            var mouse = InputSystem.AddDevice<Mouse>("SmokeMouse");
            var touch = InputSystem.AddDevice<Touchscreen>("SmokeTouch");
            Canvas.ForceUpdateCanvases();
            Physics2D.SyncTransforms();
            var screen = views.Cam.WorldToScreenPoint(producer.transform.position);
            var point = new Vector2(screen.x, screen.y);
            try
            {
                press.ApplyBindingOverride(0, mouse.path + "/leftButton"); press.ApplyBindingOverride(1, "");
                position.ApplyBindingOverride(0, mouse.path + "/position"); position.ApplyBindingOverride(1, "");
                for (int i = 0; i < 5; i++)
                {
                    // Down and up deliberately land in the same Input System update.
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
                    InputSystem.Update();
                    var created = model.Grid.Cast<GridPawn>().OfType<Appliance>().Single();
                    Require(created != null, "same-update mouse tap produces exactly one item");
                    model.UpdateGridPawn(created, true); handler.DestroyPawn(created);
                }
                press.ApplyBindingOverride(0, ""); press.ApplyBindingOverride(1, touch.path + "/primaryTouch/press");
                position.ApplyBindingOverride(0, ""); position.ApplyBindingOverride(1, touch.path + "/primaryTouch/position");
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, position = point, phase = UnityEngine.InputSystem.TouchPhase.Began });
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, position = point, phase = UnityEngine.InputSystem.TouchPhase.Ended });
                InputSystem.Update();
                var item = model.Grid.Cast<GridPawn>().OfType<Appliance>().Single();
                Require(item != null, "same-update primary touch tap produces exactly one item");
                model.UpdateGridPawn(item, true); handler.DestroyPawn(item);
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 2, position = point, phase = UnityEngine.InputSystem.TouchPhase.Began });
                InputSystem.QueueStateEvent(touch, new TouchState { touchId = 2, position = point, phase = UnityEngine.InputSystem.TouchPhase.Canceled });
                InputSystem.Update();
                Require(!model.Grid.Cast<GridPawn>().OfType<Appliance>().Any(), "canceled primary touch does not produce an item");
            }
            finally
            {
                press.RemoveAllBindingOverrides(); position.RemoveAllBindingOverrides();
                InputSystem.RemoveDevice(mouse); InputSystem.RemoveDevice(touch);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Key + "Active", false)) return;
            try
            {
                var error = SessionState.GetString(Key + "Error", "");
                if (error.Length > 0) throw new Exception(error);
                if (EditorApplication.timeSinceStartup - SessionState.GetFloat(Key + "Started", 0) > 120)
                    throw new TimeoutException("Gameplay smoke did not finish within 120 seconds.");
                int stage = SessionState.GetInt(Key + "Stage", 1);
                if (stage == 90)
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                    PlayerSettings.companyName = SessionState.GetString(Key + "Company", "LocalMerge");
                    PlayerSettings.productName = SessionState.GetString(Key + "Product", "Merge Sandbox");
                    SessionState.SetBool(Key + "Active", false);
                    File.WriteAllText("Builds/smoke-result.json", "{\"succeeded\":true,\"engine\":\"Unity Play Mode\"}");
                    EditorApplication.Exit(0);
                    return;
                }
                if (!EditorApplication.isPlaying) return;
                if (stage == 1)
                {
                    var main = UnityEngine.Object.FindObjectOfType<MainUIView>();
                    if (main == null) return;
                    main.PlayButton.onClick.Invoke();
                    SessionState.SetInt(Key + "Stage", 2);
                    return;
                }
                var context = UnityEngine.Object.FindObjectOfType<SceneContext>();
                var views = UnityEngine.Object.FindObjectOfType<GridView>();
                if (views == null || context == null) return;
                var container = context.SceneContainer;
                var model = container.Resolve<IGridModel>();
                var tasks = container.Resolve<TaskPresenter>();
                if (model.Grid == null || UnityEngine.Object.FindObjectsOfType<TaskUI>().Length < 2) return;
                // Scene setup creates tasks before the loading overlay is hidden.
                if (UnityEngine.Object.FindObjectsOfType<Camera>().Any(c =>
                    c.gameObject.scene.name == "LoadScene" && c.gameObject.activeInHierarchy)) return;
                var handler = container.Resolve<GridPawnFactoryHandler>();
                var merge = container.Resolve<MergePresenter>();
                if (stage == 2)
                {
                    Require(LocalResources.Coins == 99999999 && LocalResources.HasUnlimitedEnergy, "local resources initialized");
                    Require(model.Grid.GetLength(0) == 8 && model.Grid.GetLength(1) == 8, "8x8 board bootstraps through original scene/DI flow");
                    var producer = model.Grid.Cast<GridPawn>().OfType<Producer>().First();
                    producer.Capacity = 0;
                    VerifyRapidPointerTaps(model, handler, views, producer);
                    Invoke(merge, "OnTouched", producer);
                    for (int i = 0; i < 100; i++)
                    {
                        Invoke(merge, "TryProduceAppliance", producer);
                        var created = model.Grid.Cast<GridPawn>().OfType<Appliance>().Single();
                        Require(created.Level == 1 || created.Level == 2, "generated level stays valid");
                        model.UpdateGridPawn(created, true);
                        handler.DestroyPawn(created);
                    }
                    Require(producer.Capacity == int.MaxValue && model.Grid[producer.Coordinate.x, producer.Coordinate.y] == producer,
                        "100 productions retain producer and unlimited capacity, including legacy zero capacity");
                    var a = handler.CreateGridPawn(ApplianceType.ApplianceA, 1, new Vector2Int(0, 0));
                    var b = handler.CreateGridPawn(ApplianceType.ApplianceA, 1, new Vector2Int(1, 0));
                    model.UpdateGridPawn(a, false); model.UpdateGridPawn(b, false);
                    Require(a.BoxCollider.enabled && b.BoxCollider.enabled, "recycled pawns restore colliders");
                    Physics2D.SyncTransforms();
                    var input = UnityEngine.Object.FindObjectOfType<Input.UserInput>();
                    var inputCamera = (Camera)typeof(Input.UserInput).GetField("_cam", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(input);
                    Require(inputCamera == views.Cam, "picking uses the merge scene camera");
                    Canvas.ForceUpdateCanvases();
                    var start = views.Cam.WorldToScreenPoint(a.transform.position);
                    var finish = views.Cam.WorldToScreenPoint(b.transform.position);
                    Invoke(input, "Begin", new Vector2(start.x, start.y));
                    Require((GridPawn)typeof(Input.UserInput).GetField("_activePawn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(input) == a,
                        "screen-coordinate press picks the intended pawn");
                    Invoke(input, "Move", new Vector2(finish.x, finish.y));
                    Invoke(input, "End", true);
                    Require(model.Grid[0, 0] == null && model.Grid[1, 0] is Appliance result && result.Level == 2,
                        "actual screen-coordinate drag/release merges through presenter and factory");
                    var saved = GridSerializer.SerializeToGridInfo();
                    Require(saved.GridPawnLevels[1, 0] == 2, "merge persisted to isolated JSON save");
                    foreach (var item in model.Grid.Cast<GridPawn>().OfType<Appliance>().ToArray())
                    { model.UpdateGridPawn(item, true); handler.DestroyPawn(item); }
                    SessionState.SetInt(Key + "Stage", 3);
                    return;
                }
                if (stage == 3)
                {
                    var task = UnityEngine.Object.FindObjectsOfType<TaskUI>().OrderBy(t => t.TaskID).First();
                    SessionState.SetInt(Key + "Task", task.TaskID);
                    int x = 0;
                    foreach (var goal in task.ActiveGoals)
                    {
                        var pawn = handler.CreateGridPawn(goal.Goal.ApplianceType, goal.Goal.Level, new Vector2Int(x++, 0));
                        model.UpdateGridPawn(pawn, false);
                    }
                    tasks.UpdateTasks();
                    Require(task.DoneButton.interactable, "real order becomes ready when its items exist");
                    task.DoneButton.onClick.Invoke();
                    task.DoneButton.onClick.Invoke(); // A repeated submit must not consume twice.
                    SessionState.SetFloat(Key + "Wait", (float)EditorApplication.timeSinceStartup);
                    SessionState.SetInt(Key + "Stage", 4);
                    return;
                }
                if (stage == 4)
                {
                    if (EditorApplication.timeSinceStartup - SessionState.GetFloat(Key + "Wait", 0) < 1f) return;
                    int id = SessionState.GetInt(Key + "Task", 0);
                    Require(PlayerPrefs.GetString("CompletedTasks").Split(',').Count(s => s == id.ToString()) == 1,
                        "order submission completes and persists once");
                    Require(model.Grid.Cast<GridPawn>().OfType<Appliance>().Count() == 0, "order consumes exactly the required board items");
                    Require(UnityEngine.Object.FindObjectsOfType<TaskUI>().Length == 2, "next order replaces the completed one");
                    var max = handler.CreateGridPawn(ApplianceType.ApplianceA, 11, new Vector2Int(0, 0));
                    model.UpdateGridPawn(max, false);
                    Invoke(merge, "OnTouched", max);
                    Require(model.Grid[0, 0] == max && max.gameObject.activeInHierarchy, "max-level item survives selection for final order");
                    model.UpdateGridPawn(max, true); handler.DestroyPawn(max);
                    var bounds = views.GridSprite.bounds;
                    var min = views.Cam.WorldToScreenPoint(bounds.min);
                    var maxPoint = views.Cam.WorldToScreenPoint(bounds.max);
                    Require(min.x >= Screen.safeArea.xMin && maxPoint.x <= Screen.safeArea.xMax && min.y >= Screen.safeArea.yMin && maxPoint.y <= Screen.safeArea.yMax,
                        "actual board camera bounds fit within the safe area");
                    Require(model.Grid.Cast<GridPawn>().OfType<Producer>().Count() == 5, "all five existing starting producers remain available");
                    SessionState.SetInt(Key + "Stage", 90);
                    EditorApplication.ExitPlaymode();
                }
            }
            catch (Exception ex)
            {
                SessionState.SetBool(Key + "Active", false);
                PlayerSettings.companyName = SessionState.GetString(Key + "Company", "LocalMerge");
                PlayerSettings.productName = SessionState.GetString(Key + "Product", "Merge Sandbox");
                File.WriteAllText("Builds/smoke-result.json", "{\"succeeded\":false}");
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }
    }
}
