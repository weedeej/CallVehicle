using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using UnityEngine;
using ScheduleOne.UI.Phone;
using MelonLoader;
using CallVehicle.VehicleController;
using System.Collections;
using ScheduleOne.GameTime;
using CallVehicle.Utilities;

namespace CallVehicle.Phone
{
    public struct EntryData
    {
        public string Name;
        public string ID;
        public float Distance;
        public string Color;
    }

    public class CallVehicleApp : App<CallVehicleApp>
    {
        private CallVehicleAppUI appUI;

        private EntryData? currentSelectedEntry = null;
        private Coroutine entriesUpdater;
        private const float LIST_UPDATE_INTERVAL = 5.0f;
        private bool uiInitialized = false;
        private VehicleControl vehicleControl;

        private List<EntryData> vehicleEntries = new List<EntryData>();

        private void DefineAppSettings()
        {
            AppName = "Call Vehicle";
            IconLabel = "Call Vehicle";
            Orientation = EOrientation.Horizontal;
            AvailableInTutorial = true;
            AppIcon = ModUtilities.SpriteFromImage("CallVehicleIcon.png");
        }


        /// <summary>
        /// Fetches current vehicle data and updates the UI list.
        /// </summary>
        private void UpdateUIEntries()
        {
            if (!uiInitialized || appUI == null)
            {
                MelonLogger.Warning("CallVehicle: UpdateUIEntries called before UI was fully initialized.");
                return;
            }
            if (appUI.ListViewContent == null) return;
            appUI.ClearListItems();

            vehicleEntries.Clear();

            vehicleControl.ownedVehicles.ForEach((veh) =>
            {
                if (veh == null) return;
                string color = veh.OwnedColor.ToString();
                string displayName = $"{veh.VehicleName ?? "Unknown Name"} ({color})";
                float distance = veh.DistanceToLocalCamera;

                EntryData newEntry = new EntryData
                {
                    Name = displayName,
                    ID = veh.SaveFolderName ?? $"UnknownID_{Guid.NewGuid()}",
                    Distance = distance,
                    Color = color
                };

                vehicleEntries.Add(newEntry);

                if (appUI.ListViewContent == null)
                    return;
                appUI.AddListItem(newEntry);
            });
        }

        private IEnumerator UpdateEntries()
        {
            yield return new WaitUntil(() => uiInitialized);

            while (true)
            {
                UpdateUIEntries();
                yield return new WaitForSeconds(LIST_UPDATE_INTERVAL);
            }
        }


        protected override void Awake()
        {
            DefineAppSettings();
            base.Awake();

            CreateBaseContainer();
        }

        private void CreateBaseContainer()
        {
            if (PlayerSingleton<AppsCanvas>.Instance == null)
            {
                appContainer = null;
                return;
            }
            GameObject containerGO = new GameObject($"{AppName}_Container");
            try { containerGO.transform.SetParent(PlayerSingleton<AppsCanvas>.Instance.transform, false); }
            catch (Exception ex) { Destroy(containerGO); appContainer = null; return; }
            appContainer = containerGO.AddComponent<RectTransform>();
            if (appContainer == null) { Destroy(containerGO); return; }
            appContainer.anchorMin = Vector2.zero; appContainer.anchorMax = Vector2.one;
            appContainer.offsetMin = Vector2.zero; appContainer.offsetMax = Vector2.zero;
            appContainer.localScale = Vector3.one;
        }
        private void ShowOverview(EntryData? entryData)
        {
            if (!uiInitialized || appUI == null) return;

            currentSelectedEntry = entryData;

            if (appUI.OverviewInitialPanel == null || appUI.OverviewDetailPanel == null) return;

            bool isEntrySelected = entryData.HasValue;

            appUI.OverviewInitialPanel.SetActive(!isEntrySelected);
            appUI.OverviewDetailPanel.SetActive(isEntrySelected);

            if (isEntrySelected)
            {
                EntryData data = entryData.Value;
                if (appUI.OverviewNameText != null) appUI.OverviewNameText.text = data.Name;
                if (appUI.OverviewIdText != null) appUI.OverviewIdText.text = data.ID;
                if (appUI.OverviewDistanceText != null) appUI.OverviewDistanceText.text = data.Distance.ToString("F1") + "km";
                if (appUI.OverviewColorText != null) appUI.OverviewColorText.text = data.Color;

                string currentTimeString = TimeManager.Instance.CurrentTime >= 1800 ? "Night" : "Day";
                if (appUI.CostPricePerKmText != null) appUI.CostPricePerKmText.text = "$" + Preferences.GetPrefValue<int>(PreferenceFields.PricePerKm).ToString();
                if (appUI.CostServiceChargeText != null)
                {
                    float serviceCharge = currentTimeString == "Night" ? Preferences.GetPrefValue<int>(PreferenceFields.ServiceChargeNight) : Preferences.GetPrefValue<int>(PreferenceFields.ServiceChargeDay);
                    appUI.CostServiceChargeText.text = $"({currentTimeString}) $" + serviceCharge.ToString();
                }
                if (appUI.CostTotalCostText != null)
                {
                    int totalCost = GetCallCost(data);
                    appUI.CostTotalCostText.text = "$" + totalCost.ToString();
                }
            }
        }

        private void OnCallVehicleClicked()
        {
            if (currentSelectedEntry.HasValue)
            {
                EntryData data = currentSelectedEntry.Value;

                int totalCost = GetCallCost(currentSelectedEntry.Value);

                vehicleControl.CallVehicle(data.ID, totalCost);
            }
            else { MelonLogger.Warning("Call Vehicle clicked but no entry selected."); }
        }

        private static int GetCallCost(EntryData data)
        {
            string currentTimeString = TimeManager.Instance.CurrentTime >= 1800 ? "Night" : "Day";
            float totalCost = (
                data.Distance * Preferences.GetPrefValue<int>(PreferenceFields.PricePerKm))
                + (currentTimeString == "Night" ?
                Preferences.GetPrefValue<int>(PreferenceFields.ServiceChargeNight) :
                Preferences.GetPrefValue<int>(PreferenceFields.ServiceChargeDay));
            return (int)Math.Round(totalCost, 0);
        }
        protected override void Start()
        {
            MelonLogger.Msg("CallVehicle: Start - Starting.");
            base.Start();
            vehicleControl = new VehicleControl();

            if (appContainer != null && appUI == null)
            {
                appUI = new CallVehicleAppUI();

                Action<EntryData?> entrySelectedCallback = ShowOverview;
                Action callVehicleCallback = OnCallVehicleClicked;

                appUI.InitializeUI(appContainer, entrySelectedCallback, callVehicleCallback);

                uiInitialized = true;

            }
            else if (appContainer == null)
            {
                MelonLogger.Error("CallVehicle: Start - Cannot initialize UI because appContainer is null.");
            }
        }

        public override void SetOpen(bool open)
        {
            if (appContainer == null) return;
            if (appContainer.gameObject == null) return;

            try { base.SetOpen(open); }
            catch (Exception ex) { MelonLogger.Error($"CallVehicle: Error occurred during base.SetOpen({open}): {ex.ToString()}");
            if (open && !this.isOpen) { appContainer.gameObject.SetActive(false); } return; }

            if (!uiInitialized || appUI == null) return;

            if (this.isOpen)
            {
                ShowOverview(null);

                if (appUI.ListScrollRect != null) { appUI.ListScrollRect.normalizedPosition = new Vector2(0, 1); };


                UpdateUIEntries();

                if (entriesUpdater == null) { entriesUpdater = StartCoroutine(UpdateEntries());}
            }
            else
            {
                if (entriesUpdater != null) { StopCoroutine(entriesUpdater); entriesUpdater = null; }
            }
        }

        protected override void OnDestroy()
        {
            // Stop coroutine on destroy
            if (entriesUpdater != null) { StopCoroutine(entriesUpdater); entriesUpdater = null; }

            base.OnDestroy();

            if (AppIcon != null && AppIcon.texture != null) { if (AppIcon.texture.width == 1 && AppIcon.texture.height == 1) { Destroy(AppIcon.texture); } }

            appUI = null;
            uiInitialized = false;
        }
    }
}
