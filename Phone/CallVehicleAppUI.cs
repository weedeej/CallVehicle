using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using TMPro;
using ScheduleOne.GameTime;
using ScheduleOne.DevUtilities;

namespace CallVehicle.Phone
{
    public class CallVehicleAppUI
    {
        private readonly Color bgColor = new Color32(45, 55, 72, 255);
        private readonly Color panelColor = new Color32(55, 65, 82, 255);
        private readonly Color textColor = new Color32(226, 232, 240, 255);
        private readonly Color accentColor = new Color32(56, 178, 172, 255);
        private readonly Color buttonTextColor = new Color32(226, 232, 240, 255);
        private readonly Color fadedLineColor = new Color32(226, 232, 240, 100);

        public TextMeshProUGUI TimeText { get; private set; }
        public GameObject OverviewInitialPanel { get; private set; }
        public GameObject OverviewDetailPanel { get; private set; }
        public TextMeshProUGUI OverviewNameText { get; private set; }
        public TextMeshProUGUI OverviewIdText { get; private set; }
        public TextMeshProUGUI OverviewDistanceText { get; private set; }
        public TextMeshProUGUI OverviewColorText { get; private set; }
        public TextMeshProUGUI CostPricePerKmText { get; private set; }
        public TextMeshProUGUI CostServiceChargeText { get; private set; }
        public TextMeshProUGUI CostTotalCostText { get; private set; }
        public Button CallVehicleButton { get; private set; }
        public ScrollRect ListScrollRect { get; private set; }
        public Transform ListViewContent { get; private set; }

        private RectTransform parentContainer;
        private Action<EntryData?> entrySelectedCallback;
        private Action callVehicleCallback;

        private const float TOP_BAR_HEIGHT = 45f;
        private const float HORIZONTAL_PADDING = 10f;
        private const float VERTICAL_PADDING = 10f;

        /// <summary>
        /// Creates all UI elements for the GenericApp.
        /// </summary>
        public void InitializeUI(RectTransform container, Action<EntryData?> onEntrySelected, Action onCallVehicle)
        {
            if (container == null) return;
            this.parentContainer = container;
            this.entrySelectedCallback = onEntrySelected;
            this.callVehicleCallback = onCallVehicle;

            Image bgImage = parentContainer.GetComponent<Image>();
            if (bgImage == null) bgImage = parentContainer.gameObject.AddComponent<Image>();
            bgImage.color = bgColor;
            bgImage.raycastTarget = true;

            CreateTopBar();
            TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
            timeManager.onMinutePass = (Action)Delegate.Combine(timeManager.onMinutePass, new Action(this.MinPass));
            CreateMainContentArea();

            MelonLogger.Msg("AppUI: UI Initialization complete.");
        }

        private void CreateTopBar()
        {
            // ... (No changes needed in CreateTopBar) ...
            // Create an outer container for padding
            GameObject topBarOuterContainerGO = new GameObject("TopBarContainer");
            topBarOuterContainerGO.transform.SetParent(parentContainer, false);
            RectTransform topBarOuterRect = topBarOuterContainerGO.AddComponent<RectTransform>();
            topBarOuterRect.anchorMin = new Vector2(0, 1); topBarOuterRect.anchorMax = new Vector2(1, 1);
            topBarOuterRect.pivot = new Vector2(0.5f, 1);
            topBarOuterRect.sizeDelta = new Vector2(-(HORIZONTAL_PADDING * 2), TOP_BAR_HEIGHT);
            topBarOuterRect.anchoredPosition = new Vector2(0, 0);
            topBarOuterRect.localScale = Vector3.one;

            // Create the actual Top Bar with background color inside the padded container
            GameObject topBarGO = new GameObject("TopBar");
            topBarGO.transform.SetParent(topBarOuterRect, false);
            Image topBarBg = topBarGO.AddComponent<Image>();
            topBarBg.color = panelColor;
            RectTransform topBarRect = topBarGO.GetComponent<RectTransform>();
            topBarRect.anchorMin = Vector2.zero; topBarRect.anchorMax = Vector2.one;
            topBarRect.sizeDelta = Vector2.zero; topBarRect.anchoredPosition = Vector2.zero;
            topBarRect.localScale = Vector3.one;

            // --- Top Bar Content ---

            // Title Text (Left Anchored)
            GameObject titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(topBarGO.transform, false);
            // Use TextMeshProUGUI
            TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = "Call Vehicle";
            // Font assignment removed - TMP uses Font Assets
            titleText.fontSize = 20;
            titleText.color = textColor;
            // Use TMP alignment
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f); titleRect.anchorMax = new Vector2(0, 0.5f);
            titleRect.pivot = new Vector2(0, 0.5f);
            titleRect.sizeDelta = new Vector2(300, TOP_BAR_HEIGHT * 0.8f);
            titleRect.anchoredPosition = new Vector2(15, 0);

            // Time Text (Right Anchored)
            GameObject timeGO = new GameObject("TimeText");
            timeGO.transform.SetParent(topBarGO.transform, false);
            // Use TextMeshProUGUI and assign to public property
            this.TimeText = timeGO.AddComponent<TextMeshProUGUI>();
            this.TimeText.fontSize = 18;
            this.TimeText.color = textColor;
            // Use TMP alignment
            this.TimeText.alignment = TextAlignmentOptions.MidlineRight;
            RectTransform timeRect = timeGO.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(1, 0.5f); timeRect.anchorMax = new Vector2(1, 0.5f);
            timeRect.pivot = new Vector2(1, 0.5f);
            timeRect.sizeDelta = new Vector2(110, TOP_BAR_HEIGHT * 0.8f);
            timeRect.anchoredPosition = new Vector2(-15, 0);
        }

        private void MinPass()
        {
            if (NetworkSingleton<GameManager>.Instance.IsTutorial)
            {
                int num = TimeManager.Get24HourTimeFromMinSum(Mathf.RoundToInt(Mathf.Round((float)NetworkSingleton<TimeManager>.Instance.DailyMinTotal / 60f) * 60f));
                this.TimeText.text = TimeManager.Get12HourTime((float)num, true) + " " + NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
                return;
            }
            this.TimeText.text = TimeManager.Get12HourTime((float)NetworkSingleton<TimeManager>.Instance.CurrentTime, true) + " " + NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
        }

        private void CreateMainContentArea()
        {
            GameObject mainAreaGO = new GameObject("MainContentArea");
            mainAreaGO.transform.SetParent(parentContainer, false);
            RectTransform mainAreaRect = mainAreaGO.AddComponent<RectTransform>();
            mainAreaRect.anchorMin = Vector2.zero; mainAreaRect.anchorMax = Vector2.one;
            mainAreaRect.pivot = new Vector2(0.5f, 0.5f);
            mainAreaRect.offsetMin = new Vector2(HORIZONTAL_PADDING, VERTICAL_PADDING);
            mainAreaRect.offsetMax = new Vector2(-HORIZONTAL_PADDING, -(TOP_BAR_HEIGHT + VERTICAL_PADDING));
            mainAreaRect.localScale = Vector3.one;

            HorizontalLayoutGroup mainLayout = mainAreaGO.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(0, 0, 0, 0);
            mainLayout.spacing = 15;
            mainLayout.childAlignment = TextAnchor.UpperLeft;
            mainLayout.childControlHeight = true; mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = true; mainLayout.childForceExpandWidth = true;

            CreateLeftPanel(mainAreaGO.transform);
            CreateRightPanel(mainAreaGO.transform);
        }

        private void CreateLeftPanel(Transform parent)
        {
            this.ListViewContent = null;
            GameObject listViewContentGO = null;
            RectTransform contentRect = null;
            RectTransform scrollRectTransform = null;

            try
            {
                GameObject scrollViewGO = new GameObject("ListViewScrollView");
                if (scrollViewGO == null) return;

                scrollRectTransform = scrollViewGO.GetComponent<RectTransform>();
                if (scrollRectTransform == null)
                {
                    scrollRectTransform = scrollViewGO.AddComponent<RectTransform>();
                }
                if (scrollRectTransform == null) return;

                scrollViewGO.transform.SetParent(parent, false);

                Image scrollBg = scrollViewGO.AddComponent<Image>();
                if (scrollBg != null) scrollBg.color = panelColor;
                Mask scrollMask = scrollViewGO.AddComponent<Mask>();
                if (scrollMask != null) scrollMask.showMaskGraphic = false;

                LayoutElement scrollLayout = scrollViewGO.AddComponent<LayoutElement>();
                if (scrollLayout != null) scrollLayout.flexibleWidth = 1;

                listViewContentGO = new GameObject("ListViewContent");
                if (listViewContentGO == null) return;

                contentRect = listViewContentGO.AddComponent<RectTransform>();
                if (contentRect == null)
                {
                    GameObject.Destroy(listViewContentGO); 
                    return;
                }
                else
                {

                    contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.pivot = new Vector2(0.5f, 1); contentRect.sizeDelta = Vector2.zero;
                    contentRect.localScale = Vector3.one;
                }

                this.ListViewContent = listViewContentGO.transform;
                listViewContentGO.transform.SetParent(scrollViewGO.transform, false);
                VerticalLayoutGroup contentLayout = listViewContentGO.AddComponent<VerticalLayoutGroup>();
                if (contentLayout != null) {
                    contentLayout.padding = new RectOffset(8, 8, 8, 8); contentLayout.spacing = 8;
                    contentLayout.childAlignment = TextAnchor.UpperCenter; contentLayout.childControlHeight = true;
                    contentLayout.childControlWidth = true; contentLayout.childForceExpandHeight = false;
                    contentLayout.childForceExpandWidth = true;
                }
                ContentSizeFitter sizeFitter = listViewContentGO.AddComponent<ContentSizeFitter>();
                if (sizeFitter != null) sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (contentRect != null && scrollRectTransform != null)
                {
                    this.ListScrollRect = scrollViewGO.AddComponent<ScrollRect>();
                    if (this.ListScrollRect != null)
                    {
                        this.ListScrollRect.content = contentRect; 
                        this.ListScrollRect.viewport = scrollRectTransform; 
                        this.ListScrollRect.horizontal = false; this.ListScrollRect.vertical = true;
                        this.ListScrollRect.movementType = ScrollRect.MovementType.Clamped;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"AppUI: CreateLeftPanel - EXCEPTION: {ex.ToString()}");
                this.ListViewContent = null;
                MelonLogger.Error("AppUI: CreateLeftPanel - ListViewContent set to null due to exception.");
            }
        }

        private void CreateRightPanel(Transform parent)
        {
            GameObject rightPanelGO = new GameObject("OverviewPanel");
            rightPanelGO.transform.SetParent(parent, false);
            Image panelBg = rightPanelGO.AddComponent<Image>();
            panelBg.color = panelColor;
            RectTransform panelRect = rightPanelGO.GetComponent<RectTransform>();
            LayoutElement panelLayout = rightPanelGO.AddComponent<LayoutElement>();
            panelLayout.flexibleWidth = 1; 

            GameObject contentContainer = new GameObject("OverviewContentContainer");
            contentContainer.transform.SetParent(panelRect, false);
            RectTransform contentContainerRect = contentContainer.AddComponent<RectTransform>();
            contentContainerRect.anchorMin = Vector2.zero; contentContainerRect.anchorMax = Vector2.one;
            contentContainerRect.offsetMin = new Vector2(10, 10);
            contentContainerRect.offsetMax = new Vector2(-10, -10);
            contentContainerRect.localScale = Vector3.one;

            this.OverviewInitialPanel = new GameObject("OverviewInitialPanel");
            this.OverviewInitialPanel.transform.SetParent(contentContainerRect, false);
            RectTransform initialRect = this.OverviewInitialPanel.AddComponent<RectTransform>();
            initialRect.anchorMin = Vector2.zero; initialRect.anchorMax = Vector2.one;
            initialRect.offsetMin = Vector2.zero; initialRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup initialLayout = this.OverviewInitialPanel.AddComponent<VerticalLayoutGroup>();
            initialLayout.padding = new RectOffset(5, 5, 5, 5); initialLayout.spacing = 8;
            initialLayout.childAlignment = TextAnchor.UpperCenter; initialLayout.childControlHeight = false;
            initialLayout.childControlWidth = true; initialLayout.childForceExpandHeight = false;
            initialLayout.childForceExpandWidth = true;

            GameObject initialTitleGO = new GameObject("InitialTitle");
            initialTitleGO.transform.SetParent(initialLayout.transform, false);

            TextMeshProUGUI initialTitleText = initialTitleGO.AddComponent<TextMeshProUGUI>();
            initialTitleText.text = "Entry overview";
            initialTitleText.fontSize = 18;
            initialTitleText.color = textColor;

            initialTitleText.alignment = TextAlignmentOptions.Center;
            LayoutElement initialTitleLayout = initialTitleGO.AddComponent<LayoutElement>();
            initialTitleLayout.minHeight = 30;

            GameObject initialTextGO = new GameObject("InitialText");
            initialTextGO.transform.SetParent(initialLayout.transform, false);

            TextMeshProUGUI initialText = initialTextGO.AddComponent<TextMeshProUGUI>();
            initialText.text = "Select an entry.\n\nIf your vehicle isn't moving, Consider moving away from it.";
            initialText.fontSize = 20;
            initialText.color = textColor;

            initialText.alignment = TextAlignmentOptions.Center;
            initialText.fontStyle = FontStyles.Italic; // Use TMP FontStyles
            LayoutElement initialTextLayout = initialTextGO.AddComponent<LayoutElement>();
            initialTextLayout.minHeight = 60;
            initialTextLayout.flexibleHeight = 1;

            this.OverviewDetailPanel = new GameObject("OverviewDetailPanel");
            this.OverviewDetailPanel.transform.SetParent(contentContainerRect, false);
            RectTransform detailRect = this.OverviewDetailPanel.AddComponent<RectTransform>();
            detailRect.anchorMin = Vector2.zero; detailRect.anchorMax = Vector2.one;
            detailRect.offsetMin = Vector2.zero; detailRect.offsetMax = Vector2.zero;

            GameObject detailTitleGO = new GameObject("DetailTitle");
            detailTitleGO.transform.SetParent(detailRect, false);

            TextMeshProUGUI detailTitleText = detailTitleGO.AddComponent<TextMeshProUGUI>();
            detailTitleText.text = "Entry overview";
            detailTitleText.fontSize = 18;
            detailTitleText.color = textColor;

            detailTitleText.alignment = TextAlignmentOptions.Center;
            RectTransform detailTitleRect = detailTitleGO.GetComponent<RectTransform>();
            detailTitleRect.anchorMin = new Vector2(0, 1); detailTitleRect.anchorMax = new Vector2(1, 1);
            detailTitleRect.pivot = new Vector2(0.5f, 1); detailTitleRect.sizeDelta = new Vector2(0, 30);
            detailTitleRect.anchoredPosition = new Vector2(0, -5);

            float detailLineStartY = 35f;

            this.OverviewNameText = CreateDetailLine(detailRect, "Name", 0, detailLineStartY);
            this.OverviewIdText = CreateDetailLine(detailRect, "ID", 1, detailLineStartY);
            this.OverviewDistanceText = CreateDetailLine(detailRect, "Distance", 2, detailLineStartY);
            this.OverviewColorText = CreateDetailLine(detailRect, "Color", 3, detailLineStartY);

            float costBreakdownLineStartY = 220f;
            GameObject detailTextGO = new GameObject("CostBreakdownTitle");
            detailTextGO.transform.SetParent(detailRect, false);
            TextMeshProUGUI detailText = detailTextGO.AddComponent<TextMeshProUGUI>();
            detailText.text = "Cost breakdown";
            detailText.fontSize = 18;
            detailText.color = textColor;

            detailText.alignment = TextAlignmentOptions.Center;
            RectTransform detailTextRect = detailTextGO.GetComponent<RectTransform>();
            detailTextRect.anchorMin = new Vector2(0, 1); detailTextRect.anchorMax = new Vector2(1, 1);
            detailTextRect.pivot = new Vector2(0.5f, 1);
            detailTextRect.sizeDelta = new Vector2(0, 30);
            detailTextRect.anchoredPosition = new Vector2(0, -185f);
            this.CostPricePerKmText = CreateDetailLine(detailRect, "Price per km", 0, costBreakdownLineStartY);
            this.CostServiceChargeText = CreateDetailLine(detailRect, "Service charge", 1, costBreakdownLineStartY);
            this.CostTotalCostText = CreateDetailLine(detailRect, "Total Cost (rounded)", 2, costBreakdownLineStartY, true);

            GameObject callButtonGO = new GameObject("CallVehicleButton");
            callButtonGO.transform.SetParent(detailRect, false);

            Image callButtonBg = callButtonGO.AddComponent<Image>();
            callButtonBg.color = accentColor;
            this.CallVehicleButton = callButtonGO.AddComponent<Button>();

            RectTransform callButtonRect = callButtonGO.GetComponent<RectTransform>();
            callButtonRect.anchorMin = new Vector2(1, 0);
            callButtonRect.anchorMax = new Vector2(1, 0);
            callButtonRect.pivot = new Vector2(1, 0);

            callButtonRect.sizeDelta = new Vector2(180, 32);
            callButtonRect.anchoredPosition = new Vector2(-5, 5);
            callButtonRect.localScale = Vector3.one;

            GameObject callButtonTextGO = new GameObject("Text");
            callButtonTextGO.transform.SetParent(callButtonGO.transform, false);

            TextMeshProUGUI callButtonText = callButtonTextGO.AddComponent<TextMeshProUGUI>();

            callButtonText.text = "Call Vehicle >";
            callButtonText.fontSize = 20;
            callButtonText.fontStyle = FontStyles.Bold;
            callButtonText.color = buttonTextColor;

            callButtonText.alignment = TextAlignmentOptions.Center;
            RectTransform callTextRect = callButtonTextGO.GetComponent<RectTransform>();
            callTextRect.anchorMin = Vector2.zero; callTextRect.anchorMax = Vector2.one;
            callTextRect.offsetMin = Vector2.zero; callTextRect.offsetMax = Vector2.zero;

            this.CallVehicleButton.onClick.AddListener(() => { if (callVehicleCallback != null) callVehicleCallback(); });

            this.OverviewDetailPanel.SetActive(false);
        }
        private TextMeshProUGUI CreateDetailLine(RectTransform parent, string label, int index, float startY, bool boldValue = false)
        {
            float lineHeight = 30f;
            float spacing = 8f;

            GameObject lineGO = new GameObject(label.Replace(":", "") + "Line");
            lineGO.transform.SetParent(parent, false);
            RectTransform lineRect = lineGO.AddComponent<RectTransform>();

            lineRect.anchorMin = new Vector2(0, 1);
            lineRect.anchorMax = new Vector2(1, 1);
            lineRect.pivot = new Vector2(0.5f, 1);
            lineRect.anchoredPosition = new Vector2(0, -(startY + index * (lineHeight + spacing)));
            lineRect.sizeDelta = new Vector2(0, lineHeight);
            lineRect.localScale = Vector3.one;

            HorizontalLayoutGroup lineLayout = lineGO.AddComponent<HorizontalLayoutGroup>();
            lineLayout.padding = new RectOffset(0, 0, 0, 0);
            lineLayout.spacing = 8;
            lineLayout.childControlHeight = true;
            lineLayout.childControlWidth = true;
            lineLayout.childForceExpandHeight = false;
            lineLayout.childForceExpandWidth = false;

            lineLayout.childAlignment = TextAnchor.MiddleCenter;

            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(lineLayout.transform, false);
            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.fontStyle = FontStyles.Bold;
            labelText.color = textColor;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject lineImageGO = new GameObject("StretchingLine");
            lineImageGO.transform.SetParent(lineLayout.transform, false);
            Image lineImage = lineImageGO.AddComponent<Image>();
            lineImage.color = fadedLineColor;

            LayoutElement lineLayoutElement = lineImageGO.AddComponent<LayoutElement>();

            lineLayoutElement.flexibleWidth = 1;
            lineLayoutElement.minHeight = 1;
            lineLayoutElement.preferredHeight = 1;
            lineLayoutElement.flexibleHeight = 0;

            GameObject valueGO = new GameObject("Value");
            valueGO.transform.SetParent(lineLayout.transform, false);
            TextMeshProUGUI valueText = valueGO.AddComponent<TextMeshProUGUI>();
            valueText.text = "-";
            valueText.fontSize = 18;
            valueText.color = textColor;
            valueText.alignment = TextAlignmentOptions.MidlineRight;
            valueText.fontStyle = boldValue ? FontStyles.Bold : FontStyles.Normal;

            return valueText;
        }

        /// <summary>
        /// Removes all existing items from the list view.
        /// </summary>
        public void ClearListItems()
        {
            if (ListViewContent == null)
            {
                // Changed to Error for higher visibility
                return;
            }

            // Destroy existing children
            // Use a loop that accounts for removing items while iterating
            for (int i = ListViewContent.childCount - 1; i >= 0; i--)
            {
                Transform child = ListViewContent.GetChild(i);
                if (child != null)
                {
                    // Use DestroyImmediate if Destroy doesn't work fast enough before repopulation
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Creates and adds a single list item UI element.
        /// </summary>
        /// <param name="entryData">The data for the list item.</param>
        public void AddListItem(EntryData entryData)
        {
            if (ListViewContent == null)
            {
                return;
            }

            GameObject itemGO = new GameObject($"ListItem_{entryData.Name}");
            itemGO.transform.SetParent(ListViewContent, false);

            Image itemImage = itemGO.AddComponent<Image>();
            itemImage.color = bgColor;

            Button itemButton = itemGO.AddComponent<Button>();
            Navigation nav = itemButton.navigation;
            nav.mode = Navigation.Mode.None;
            itemButton.navigation = nav;

            LayoutElement layoutElement = itemGO.AddComponent<LayoutElement>();
            layoutElement.minHeight = 35;

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(itemGO.transform, false);
            TextMeshProUGUI itemText = textGO.AddComponent<TextMeshProUGUI>();
            itemText.text = entryData.Name;
            itemText.fontSize = 16;
            itemText.color = textColor;
            itemText.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 4);
            textRect.offsetMax = new Vector2(-10, -4);

            itemButton.onClick.AddListener(() => {
                if (entrySelectedCallback != null) entrySelectedCallback(entryData);
            });
        }
    }
}
