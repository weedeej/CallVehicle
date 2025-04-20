using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using UnityEngine;
using ScheduleOne.UI.Phone;
using MelonLoader;
// Assuming VehicleControl is in this namespace based on usage
using CallVehicle.VehicleController;
using System; // Added for Exception, Action, Guid
using System.Collections;
using ScheduleOne.GameTime; // Added for List

// Namespace from user file
namespace CallVehicle.Phone
{
    // Renamed from GenericPhoneApp (as in user file)
    // NOTE: This struct might be better placed outside GenericApp if used elsewhere
    public struct EntryData
    {
        public string Name;
        public string ID;
        public float Distance; // Assuming distance is numeric
        public string Color;
    }

    public class GenericApp : App<GenericApp>
    {
        // --- UI Handler ---
        private AppUI appUI; // Handles UI creation and element references

        // --- App State ---
        private EntryData? currentSelectedEntry = null; // Nullable struct to track selection
        private Coroutine entriesUpdater;
        private const float LIST_UPDATE_INTERVAL = 5.0f;
        private bool uiInitialized = false; // Flag to track UI setup completion
        private VehicleControl vehicleControl;

        // --- Data --- // Renamed section from user file
        private List<EntryData> vehicleEntries = new List<EntryData>();

        // Define App settings
        private void DefineAppSettings()
        {
            AppName = "Generic App";
            IconLabel = "Generic";
            Orientation = EOrientation.Horizontal;
            AvailableInTutorial = true;

            // Create icon sprite
            try
            {
                Texture2D whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
                AppIcon = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                if (AppIcon == null) MelonLogger.Error("GenericApp: Failed to create AppIcon Sprite.");
            }
            catch (Exception ex) { MelonLogger.Error($"GenericApp: Error creating AppIcon: {ex.Message}"); }
        }


        /// <summary>
        /// Fetches current vehicle data and updates the UI list.
        /// </summary>
        private void UpdateUIEntries()
        {
            // Wait until UI is initialized
            if (!uiInitialized || appUI == null)
            {
                MelonLogger.Warning("GenericApp: UpdateUIEntries called before UI was fully initialized.");
                return;
            }

            // Check ListViewContent specifically before clearing/adding
            MelonLogger.Msg($"GenericApp: UpdateUIEntries - Checking appUI.ListViewContent before update. Is it null? {(appUI.ListViewContent == null)}");
            if (appUI.ListViewContent == null)
            {
                MelonLogger.Error("GenericApp: UpdateUIEntries - Attempting to update list, but appUI.ListViewContent is NULL!");
                // Consider retrying initialization or logging more context here
                return;
            }

            // Instantiate VehicleControl here to get fresh data each time
            MelonLogger.Msg($"GenericApp: Found {vehicleControl.ownedVehicles.Count} owned vehicles.");

            // Clear previous entries
            MelonLogger.Msg("GenericApp: Calling appUI.ClearListItems()...");
            appUI.ClearListItems();

            // Clear the local list before repopulating
            vehicleEntries.Clear();

            // Populate temporary list and add to UI
            MelonLogger.Msg("GenericApp: Populating UI list...");
            vehicleControl.ownedVehicles.ForEach((veh) =>
            {
                if (veh == null)
                {
                    MelonLogger.Warning("GenericApp: Encountered null vehicle in ownedVehicles list.");
                    return;
                }
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

                vehicleEntries.Add(newEntry); // Add to local list

                if (appUI.ListViewContent == null)
                {
                    MelonLogger.Error("GenericApp: UpdateUIEntries - appUI.ListViewContent became NULL during list population loop!");
                    return;
                }
                appUI.AddListItem(newEntry); // Add directly to UI
            });

            MelonLogger.Msg($"GenericApp: Updated UI list with {vehicleEntries.Count} entries.");
        }

        // Coroutine to periodically update entries
        private IEnumerator UpdateEntries()
        {
            MelonLogger.Msg("GenericApp: List update coroutine started. Waiting for UI initialization...");
            // Wait until UI is initialized 
            yield return new WaitUntil(() => uiInitialized);
            MelonLogger.Msg("GenericApp: UI initialized, starting update loop.");

            while (true)
            {
                UpdateUIEntries();
                yield return new WaitForSeconds(LIST_UPDATE_INTERVAL);
            }
        }


        // Awake is called when the script instance is being loaded
        protected override void Awake()
        {
            MelonLogger.Msg("GenericApp: Awake - Starting.");
            DefineAppSettings();
            base.Awake();

            // Create the base container GameObject here
            CreateBaseContainer(); // Creates and assigns appContainer

            // MOVED UI Initialization to Start()

            MelonLogger.Msg($"GenericApp: End of Awake. appContainer is {(appContainer == null ? "NULL" : "Assigned")}");
            if (appContainer != null && appContainer.gameObject.activeSelf) { /* Base SetOpen handles initial state */ }
        }

        // Creates ONLY the main container (full screen), AppUI handles the rest
        private void CreateBaseContainer()
        {
            // ... (CreateBaseContainer remains the same) ...
            if (PlayerSingleton<AppsCanvas>.Instance == null)
            {
                MelonLogger.Error("GenericApp: CreateBaseContainer - AppsCanvas instance is null!");
                appContainer = null; // Use the protected field from App<T>
                return;
            }
            GameObject containerGO = new GameObject($"{AppName}_Container");
            try { containerGO.transform.SetParent(PlayerSingleton<AppsCanvas>.Instance.transform, false); }
            catch (Exception ex) { MelonLogger.Error($"GenericApp: CreateBaseContainer - Failed to parent container: {ex.Message}"); Destroy(containerGO); appContainer = null; return; }
            appContainer = containerGO.AddComponent<RectTransform>();
            if (appContainer == null) { MelonLogger.Error("GenericApp: CreateBaseContainer - Failed to AddComponent<RectTransform>!"); Destroy(containerGO); return; }
            appContainer.anchorMin = Vector2.zero; appContainer.anchorMax = Vector2.one;
            appContainer.offsetMin = Vector2.zero; appContainer.offsetMax = Vector2.zero;
            appContainer.localScale = Vector3.one;
            MelonLogger.Msg("GenericApp: CreateBaseContainer - Successfully created appContainer.");
        }

        // --- Interaction Methods ---

        // Called when a list item is clicked (passed as callback to AppUI)
        private void ShowOverview(EntryData? entryData)
        {
            // Wait until UI is initialized
            if (!uiInitialized || appUI == null)
            {
                MelonLogger.Warning("GenericApp: ShowOverview called before UI was fully initialized.");
                return;
            }

            currentSelectedEntry = entryData; // Update state

            // Check if UI handler panels are ready (already checked appUI)
            if (appUI.OverviewInitialPanel == null || appUI.OverviewDetailPanel == null)
            {
                MelonLogger.Error("GenericApp: ShowOverview - AppUI Overview panels are not initialized!");
                return;
            }

            bool isEntrySelected = entryData.HasValue;

            // Update UI visibility via AppUI references
            appUI.OverviewInitialPanel.SetActive(!isEntrySelected);
            appUI.OverviewDetailPanel.SetActive(isEntrySelected);

            if (isEntrySelected)
            {
                EntryData data = entryData.Value;
                // Update detail text fields via AppUI references
                if (appUI.OverviewNameText != null) appUI.OverviewNameText.text = data.Name;
                if (appUI.OverviewIdText != null) appUI.OverviewIdText.text = data.ID;
                if (appUI.OverviewDistanceText != null) appUI.OverviewDistanceText.text = data.Distance.ToString("F1") + "km";
                if (appUI.OverviewColorText != null) appUI.OverviewColorText.text = data.Color;
                // COST
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

        // Called when "Call Vehicle" button is clicked (passed as callback to AppUI)
        private void OnCallVehicleClicked()
        {
            if (currentSelectedEntry.HasValue)
            {
                EntryData data = currentSelectedEntry.Value;
                MelonLogger.Msg($"Calling Vehicle: {data.Name} (ID: {data.ID})");
                // COST
                int totalCost = GetCallCost(currentSelectedEntry.Value);
                // Call vehicle
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

        // --- Overrides ---
        protected override void Start()
        {
            MelonLogger.Msg("GenericApp: Start - Starting.");
            base.Start();
            vehicleControl = new VehicleControl();

            // MOVED UI Initialization here
            if (appContainer != null && appUI == null) // Only initialize if not already done and container exists
            {
                MelonLogger.Msg("GenericApp: Start - Instantiating AppUI...");
                appUI = new AppUI();
                MelonLogger.Msg($"GenericApp: Start - AppUI instantiated. Is it null? {(appUI == null)}");

                Action<EntryData?> entrySelectedCallback = ShowOverview;
                Action callVehicleCallback = OnCallVehicleClicked;

                MelonLogger.Msg("GenericApp: Start - Calling appUI.InitializeUI...");
                // Pass the vehicleEntries list (it's initialized in DefineAppSettings called by Awake)
                appUI.InitializeUI(appContainer, entrySelectedCallback, callVehicleCallback);
                MelonLogger.Msg("GenericApp: Start - Returned from appUI.InitializeUI.");
                MelonLogger.Msg($"GenericApp: Start - After InitializeUI, is appUI.ListViewContent null? {(appUI.ListViewContent == null)}");

                // Set flag only after successful initialization attempt
                uiInitialized = true;
                MelonLogger.Msg("GenericApp: Start - UI Initialization complete flag set.");

                // Initial population happens when SetOpen(true) is called, or by coroutine
            }
            else if (appContainer == null)
            {
                MelonLogger.Error("GenericApp: Start - Cannot initialize UI because appContainer is null.");
            }
            else if (appUI != null)
            {
                MelonLogger.Msg("GenericApp: Start - AppUI already initialized.");

            }


            // Start the update coroutine here
            if (entriesUpdater == null)
            {
                entriesUpdater = StartCoroutine(UpdateEntries());
            }
        }

        // Add logging and checks to SetOpen
        public override void SetOpen(bool open)
        {
            // --- Pre-Base Call Logging & Checks ---
            MelonLogger.Msg($"GenericApp: SetOpen called with open = {open}. Current isOpen = {this.isOpen}");
            if (appContainer == null) { MelonLogger.Error($"GenericApp: SetOpen({open}) aborted! appContainer is null."); return; }
            if (appContainer.gameObject == null) { MelonLogger.Error($"GenericApp: SetOpen({open}) aborted! appContainer.gameObject is null."); return; }
            MelonLogger.Msg($"GenericApp: Calling base.SetOpen({open}). appContainer active status before call: {appContainer.gameObject.activeSelf}");

            // --- Call Base Method ---
            try { base.SetOpen(open); } // This handles activating appContainer.gameObject
            catch (Exception ex) { MelonLogger.Error($"GenericApp: Error occurred during base.SetOpen({open}): {ex.ToString()}"); if (open && !this.isOpen) { appContainer.gameObject.SetActive(false); } return; }

            // --- Post-Base Call Logging & Logic ---
            MelonLogger.Msg($"GenericApp: Returned from base.SetOpen({open}). Property this.isOpen is now: {this.isOpen}. appContainer active status after call: {appContainer.gameObject.activeSelf}");

            // Check if UI is initialized before proceeding
            if (!uiInitialized || appUI == null) { MelonLogger.Error("GenericApp: SetOpen - UI not initialized! Cannot perform UI updates."); return; }

            if (this.isOpen)
            {
                ShowOverview(null); // Reset selection view

                // Reset scroll position via AppUI reference
                if (appUI.ListScrollRect != null) { appUI.ListScrollRect.normalizedPosition = new Vector2(0, 1); MelonLogger.Msg("GenericApp: Reset scroll position to top."); }
                else { MelonLogger.Warning("GenericApp: SetOpen - Cannot reset scroll position, appUI.ListScrollRect is null."); }

                // Trigger list update explicitly when opening
                MelonLogger.Msg("GenericApp: SetOpen(true) - Calling UpdateUIEntries...");
                UpdateUIEntries(); // Fetch and display list on open
                MelonLogger.Msg("GenericApp: SetOpen(true) - Returned from UpdateUIEntries.");

                // Ensure coroutine is running when app opens
                if (entriesUpdater == null) { entriesUpdater = StartCoroutine(UpdateEntries()); MelonLogger.Msg("GenericApp: Restarted list update coroutine on app open."); }
            }
            else // App is closing
            {
                // Stop the coroutine when the app closes
                if (entriesUpdater != null) { StopCoroutine(entriesUpdater); entriesUpdater = null; MelonLogger.Msg("GenericApp: Stopped list update coroutine because app closed."); }
            }
        }

        protected override void OnDestroy()
        {
            // Stop coroutine on destroy
            if (entriesUpdater != null) { StopCoroutine(entriesUpdater); entriesUpdater = null; }

            base.OnDestroy();

            if (AppIcon != null && AppIcon.texture != null) { if (AppIcon.texture.width == 1 && AppIcon.texture.height == 1) { Destroy(AppIcon.texture); } }

            // Optional: Clean up AppUI instance?
            appUI = null;
            uiInitialized = false;
            MelonLogger.Msg("GenericApp: OnDestroy finished.");
        }
    }
}
