using MelonLoader;
using MelonLoader.Utils;
using System.Reflection; // Required for FieldInfo

/// <summary>
/// Defines constant keys for preference entries for type safety and easy refactoring.
/// </summary>
public static class PreferenceFields
{
    public const string PricePerKm = "price_per_km";
    public const string UseCash = "use_cash";
    public const string ServiceChargeDay = "service_charge_day";
    public const string ServiceChargeNight = "service_charge_night";
}

namespace CallVehicle
{
    /// <summary>
    /// Manages loading, saving, and accessing preferences for the CallVehicle mod.
    /// </summary>
    public static class Preferences
    {
        /// <summary>
        /// Structure to hold the definition of a single preference entry.
        /// </summary>
        public struct PreferenceEntryDef
        {
            public string id; // Must match a field in PreferenceFields
            public string name; // User-friendly name shown in config/UI
            public string description; // Description shown in config/UI
            public object value; // Default value
            public Type type; // The expected data type (e.g., typeof(int), typeof(bool))
        }

        /// <summary>
        /// Structure containing all default preference definitions.
        /// Uses PreferenceEntryDef for clarity and includes the Type.
        /// </summary>
        public struct DefaultPreferences
        {
            // Ensure the 'id' matches the constants in PreferenceFields
            public PreferenceEntryDef price_per_km;
            public PreferenceEntryDef use_cash;
            public PreferenceEntryDef service_charge_day;
            public PreferenceEntryDef service_charge_night;
        }

        // Static instance holding all default preference configurations.
        public static readonly DefaultPreferences defaultPreferences = new()
        {
            price_per_km = new PreferenceEntryDef() { id = PreferenceFields.PricePerKm, name = "Price per km", description = "The amount of cash/online balance to pay for the service per kilometer.", value = 13, type = typeof(int) },
            use_cash = new PreferenceEntryDef() { id = PreferenceFields.UseCash, name = "Use Cash", description = "Use cash instead of online balance. (true/false)", value = false, type = typeof(bool) },
            service_charge_day = new PreferenceEntryDef() { id = PreferenceFields.ServiceChargeDay, name = "Service Charge (Day)", description = "Flat fee for the service during the day.", value = 500, type = typeof(int) },
            service_charge_night = new PreferenceEntryDef() { id = PreferenceFields.ServiceChargeNight, name = "Service Charge (Night)", description = "Flat fee for the service during the night.", value = 800, type = typeof(int) }
        };

        // Configuration file details
        private const string ConfigFileName = "ChauffeurPrefs.cfg";
        private const string ConfigCategoryName = "CallVehicle";

        // MelonLoader specifics
        private static MelonPreferences_Category category;
        private static MelonLogger.Instance Logger; // Logger instance for messages

        /// <summary>
        /// Initializes the Preferences system. Call this ONCE from your mod's main class.
        /// e.g., in OnApplicationStart or OnInitializeMelon.
        /// </summary>
        /// <param name="loggerInstance">Your mod's MelonLogger instance.</param>
        public static void Setup(MelonLogger.Instance loggerInstance)
        {
            Logger = loggerInstance ?? throw new ArgumentNullException(nameof(loggerInstance)); // Ensure logger is provided
            Logger.Msg($"Setting up '{ConfigCategoryName}' preferences...");

            // Get or create the MelonPreferences category
            category = MelonPreferences.GetCategory(ConfigCategoryName);
            if (category == null)
            {
                Logger.Msg($"'{ConfigCategoryName}' category not found, creating...");
                category = MelonPreferences.CreateCategory(ConfigCategoryName, "Call Vehicle Preferences");
            }
            else
            {
                Logger.Msg($"'{ConfigCategoryName}' category found.");
            }

            // Updated to use the recommended MelonEnvironment.UserDataDirectory instead of the obsolete MelonUtils.UserDataDirectory
            string filePath = Path.Combine(MelonEnvironment.UserDataDirectory, ConfigFileName);
            Logger.Msg($"Expected config file path: {filePath}");

            // Set the file path for the category, disable auto-loading for manual control
            category.SetFilePath(filePath, autoload: false);
            Logger.Msg($"Set category file path. Autoload disabled.");

            bool fileNeedsSaving = false;

            // Check if the configuration file exists
            if (!File.Exists(filePath))
            {
                Logger.Warning($"Config file not found at '{filePath}'. Creating default entries.");
                CreateDefaultEntries(); // Create all entries based on defaults
                fileNeedsSaving = true; // Mark that the new file needs to be saved
            }
            else
            {
                Logger.Msg($"Config file found. Loading entries from '{filePath}'...");
                category.LoadFromFile(false); // Load existing file without saving immediately
                Logger.Msg($"Loaded config file. Entry count before check: {category?.Entries.Count ?? -1}");

                // Check if existing file has all necessary entries, add missing ones
                if (CheckAndCreateMissingEntries())
                {
                    Logger.Msg($"Missing entries found and created in existing config.");
                    fileNeedsSaving = true; // Mark that the updated file needs saving
                }
                else
                {
                    Logger.Msg($"Existing config file contains all required entries.");
                }
            }

            // Save the file only if new entries were created (either new file or added missing)
            if (fileNeedsSaving)
            {
                Logger.Msg($"Saving preferences to '{filePath}'...");
                category.SaveToFile(false); // Save changes without reloading
                Logger.Msg($"Saved config file. Entry count after save: {category?.Entries.Count ?? -1}");
            }

            // Final verification log
            Logger.Msg($"'{ConfigCategoryName}' Preferences initialization complete.");
            LogCurrentEntries();
        }

        /// <summary>
        /// Creates all preference entries based on the 'defaultPreferences' structure.
        /// Assumes the category is clean or entries don't exist.
        /// </summary>
        private static void CreateDefaultEntries()
        {
            if (category == null) return;
            Logger.Msg($"Creating default entries...");
            FieldInfo[] properties = typeof(DefaultPreferences).GetFields();
            foreach (var prop in properties)
            {
                var entryValue = prop.GetValue(defaultPreferences);
                if (entryValue is PreferenceEntryDef preference)
                {
                    Logger.Msg($" -> Creating entry: ID='{preference.id}', Name='{preference.name}', DefaultValue='{preference.value}'");
                    // Use the correct overload for CreateEntry based on the type
                    CreateEntryWithType(preference);
                }
            }
        }

        /// <summary>
        /// Checks if all defined preferences exist in the category, creates missing ones.
        /// </summary>
        /// <returns>True if any entries were added, false otherwise.</returns>
        private static bool CheckAndCreateMissingEntries()
        {
            if (category == null) return false;

            bool entryAdded = false;
            FieldInfo[] properties = typeof(DefaultPreferences).GetFields();
            Logger.Msg($"Checking for missing entries. Expecting {properties.Length}.");

            foreach (var prop in properties)
            {
                var entryValue = prop.GetValue(defaultPreferences);
                if (entryValue is PreferenceEntryDef preference)
                {
                    if (!category.HasEntry(preference.id))
                    {
                        Logger.Warning($" -> Missing entry found: ID='{preference.id}'. Creating with default value.");
                        CreateEntryWithType(preference);
                        entryAdded = true;
                    }
                }
            }
            return entryAdded;
        }

        /// <summary>
        /// Helper to call the correct MelonPreferences.CreateEntry<T> based on the stored type.
        /// </summary>
        /// <param name="preference">The preference definition.</param>
        private static void CreateEntryWithType(PreferenceEntryDef preference)
        {
            // This is a common pattern but can be verbose. Consider reflection or a dictionary lookup if types expand.
            if (preference.type == typeof(string))
                category.CreateEntry<string>(preference.id, (string)preference.value, preference.name, preference.description);
            else if (preference.type == typeof(int))
                category.CreateEntry<int>(preference.id, (int)preference.value, preference.name, preference.description);
            else if (preference.type == typeof(float))
                category.CreateEntry<float>(preference.id, (float)preference.value, preference.name, preference.description);
            else if (preference.type == typeof(bool))
                category.CreateEntry<bool>(preference.id, (bool)preference.value, preference.name, preference.description);
            // Add other types as needed (e.g., double, enum)
            else
            {
                // Fallback or error for unsupported types
                Logger.Error($"Unsupported preference type '{preference.type}' for entry '{preference.id}'. Using string fallback.");
                category.CreateEntry<string>(preference.id, preference.value.ToString(), preference.name, preference.description);
            }
        }


        /// <summary>
        /// Gets the value of a preference entry, ensuring type safety.
        /// </summary>
        /// <typeparam name="T">The expected type of the preference.</typeparam>
        /// <param name="key">The preference ID (use PreferenceFields constants).</param>
        /// <returns>The preference value, or the default value for T if not found or type mismatch.</returns>
        public static T GetPrefValue<T>(string key)
        {
            if (category == null)
            {
                Logger.Error($"Attempted to get preference '{key}' before Preferences.Setup() was called.");
                return default(T);
            }

            if (!category.HasEntry(key))
            {
                Logger.Warning($"Attempted to get non-existent preference entry: '{key}'. Returning default value for type {typeof(T)}.");
                // Optionally, find and return the configured default value from defaultPreferences
                return GetDefaultValue<T>(key);
            }

            var entry = category.GetEntry<T>(key); // Use generic GetEntry for type safety
            if (entry == null)
            {
                // This might happen if the type T doesn't match the stored type
                Logger.Error($"Failed to get preference entry '{key}' with type {typeof(T)}. Stored type might differ. Returning default.");
                // Attempt to get the boxed value and convert, or return default
                var boxedEntry = category.GetEntry(key);
                if (boxedEntry != null)
                {
                    try
                    {
                        return (T)Convert.ChangeType(boxedEntry.BoxedValue, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Conversion failed for '{key}': {ex.Message}");
                        return GetDefaultValue<T>(key); ;
                    }
                }
                return GetDefaultValue<T>(key); ;
            }

            return entry.Value;
        }

        /// <summary>
        /// Sets the value of a preference entry and saves the file.
        /// </summary>
        /// <typeparam name="T">The type of the value being set.</typeparam>
        /// <param name="key">The preference ID (use PreferenceFields constants).</param>
        /// <param name="value">The new value to set.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool SetPrefValue<T>(string key, T value)
        {
            if (category == null)
            {
                Logger.Error($"Attempted to set preference '{key}' before Preferences.Setup() was called.");
                return false;
            }

            if (!category.HasEntry(key))
            {
                Logger.Warning($"Attempted to set non-existent preference entry: '{key}'. Cannot set value.");
                return false;
            }

            var entry = category.GetEntry<T>(key);
            if (entry == null)
            {
                Logger.Error($"Failed to get preference entry '{key}' with expected type {typeof(T)} for setting. Stored type might differ.");
                // Optionally try GetEntry(key).BoxedValue = value; but it's less safe
                return false;
            }

            try
            {
                entry.Value = value;
                category.SaveToFile(false); // Save immediately after setting
                Logger.Msg($"Set preference '{key}' to '{value}'. Saved.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to set preference '{key}' with value '{value}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Helper to get the configured default value for a given key.
        /// </summary>
        private static T GetDefaultValue<T>(string key)
        {
            FieldInfo[] properties = typeof(DefaultPreferences).GetFields();
            foreach (var prop in properties)
            {
                var entryValue = prop.GetValue(defaultPreferences);
                if (entryValue is PreferenceEntryDef preference && preference.id == key)
                {
                    try
                    {
                        // Ensure the default value is convertible to the requested type T
                        return (T)Convert.ChangeType(preference.value, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        Logger?.Error($"Failed to convert default value for '{key}' to type {typeof(T)}: {ex.Message}");
                        return default(T); // Return default(T) on conversion error
                    }
                }
            }
            Logger?.Warning($"No default value definition found for key '{key}'.");
            return default(T); // Return default(T) if key not found in defaults
        }


        /// <summary>
        /// Logs the current state of all entries in the category.
        /// </summary>
        private static void LogCurrentEntries()
        {
            if (category == null || Logger == null) return;

            Logger.Msg($"Current '{ConfigCategoryName}' Entries ({category.Entries.Count}):");
            if (category.Entries.Count == 0)
            {
                Logger.Msg(" -> No entries loaded.");
                return;
            }
            foreach (var entry in category.Entries)
            {
                Logger.Msg($" -> ID: {entry.Identifier}, Type: {entry.GetReflectedType().Name}, Value: {entry.BoxedValue}");
            }
        }
    }
}