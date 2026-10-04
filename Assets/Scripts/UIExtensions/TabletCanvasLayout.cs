using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIExtensions
{
    // Preserve existing scene references while wrapping UI in a safe-area parent.
    public class TabletCanvasLayout : MonoBehaviour
    {
        private RectTransform _safe;
        private Rect _lastArea;
        private Vector2Int _lastSize;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= Prepare;
            SceneManager.sceneLoaded += Prepare;
            LocalResources.Initialize();
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private static void Prepare(Scene scene, LoadSceneMode mode)
        {
            foreach (var canvas in FindObjectsOfType<Canvas>())
                if (canvas.isRootCanvas && canvas.GetComponent<TabletCanvasLayout>() == null)
                    canvas.gameObject.AddComponent<TabletCanvasLayout>();
        }

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = canvas.name == "Inventory_Canvas" ? 10 : 0;
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(768f, 1024f);
                scaler.matchWidthOrHeight = 0f;
            }
            var children = new Transform[transform.childCount];
            for (int i = 0; i < children.Length; i++) children[i] = transform.GetChild(i);
            _safe = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            _safe.SetParent(transform, false);
            foreach (var child in children) child.SetParent(_safe, false);
            if (canvas.name == "UI_Canvas") CreateWallet();
            UpdateArea();
        }

        private void LateUpdate()
        {
            if (_lastArea != Screen.safeArea || _lastSize.x != Screen.width || _lastSize.y != Screen.height)
                UpdateArea();
        }

        private void UpdateArea()
        {
            _lastArea = Screen.safeArea;
            _lastSize = new Vector2Int(Screen.width, Screen.height);
            _safe.anchorMin = new Vector2(_lastArea.xMin / Screen.width, _lastArea.yMin / Screen.height);
            _safe.anchorMax = new Vector2(_lastArea.xMax / Screen.width, _lastArea.yMax / Screen.height);
            _safe.offsetMin = _safe.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            ConfigureChildren();
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.localScale = Vector3.one;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }

        private void ConfigureChildren()
        {
            foreach (var rect in _safe.GetComponentsInChildren<RectTransform>(true))
            {
                switch (rect.name)
                {
                    case "TasksParent_Tr":
                        Place(rect, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(656f, 210f));
                        var row = rect.GetComponent<HorizontalLayoutGroup>();
                        row.padding = new RectOffset();
                        row.spacing = 16f;
                        row.childForceExpandWidth = row.childForceExpandHeight = false;
                        break;
                    case "Inventory_Button":
                        Place(rect, new Vector2(0.5f, 0f), new Vector2(0f, 78f), new Vector2(112f, 96f));
                        var label = rect.GetComponentInChildren<TMP_Text>();
                        if (label != null)
                        {
                            Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -58f), new Vector2(180f, 28f));
                            label.fontSize = 22f;
                            label.enableAutoSizing = false;
                        }
                        break;
                    case "Inventory_Panel": Stretch(rect, new Vector2(16f, 16f), new Vector2(-16f, -16f)); break;
                    case "BG_Panel": Stretch(rect, Vector2.zero, Vector2.zero); break;
                    case "Close_Button":
                        Place(rect, Vector2.one, new Vector2(-56f, -56f), new Vector2(96f, 96f)); break;
                    case "Scroll View": Stretch(rect, new Vector2(16f, 16f), new Vector2(-16f, -104f)); break;
                    case "Viewport": Stretch(rect, Vector2.zero, Vector2.zero); break;
                    case "InventoryPawnUIsParent_Tr":
                        var grid = rect.GetComponent<GridLayoutGroup>();
                        grid.padding = new RectOffset(12, 12, 12, 12);
                        grid.cellSize = new Vector2(96f, 96f);
                        grid.spacing = new Vector2(12f, 12f);
                        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                        grid.constraintCount = Mathf.Max(1, Mathf.FloorToInt((_safe.rect.width - 88f) / 108f));
                        var fitter = rect.GetComponent<ContentSizeFitter>() ?? rect.gameObject.AddComponent<ContentSizeFitter>();
                        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                        break;
                    case "Inventory_Text":
                        if (rect.parent.name == "Inventory_Panel")
                        {
                            Place(rect, new Vector2(0.5f, 1f), new Vector2(-32f, -48f), new Vector2(520f, 72f));
                            var text = rect.GetComponent<TMP_Text>();
                            text.fontSize = 36f;
                            text.enableAutoSizing = false;
                        }
                        break;
                    case "Play_Button":
                        Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(280f, 160f)); break;
                    case "Play_Text":
                        var playText = rect.GetComponent<TMP_Text>();
                        playText.fontSize = 48f;
                        playText.enableAutoSizing = false;
                        break;
                    case "Title_Text":
                        Place(rect, new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(700f, 200f));
                        rect.GetComponent<TMP_Text>().fontSize = 72f;
                        break;
                }
            }
        }

        private void CreateWallet()
        {
            var text = new GameObject("LocalWallet", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(_safe, false);
            Place(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(728f, 64f));
            text.text = $"Coins {LocalResources.Coins:N0}  |  Energy Unlimited";
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
        }
    }
}
