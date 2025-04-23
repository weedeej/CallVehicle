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
    public const string BypassCheckpoints = "bypass_checkpoints";
    public const string AppPrice = "app_price";
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
            public PreferenceEntryDef bypass_checkpoints;
            public PreferenceEntryDef app_price;
        }

        // Static instance holding all default preference configurations.
        public static readonly DefaultPreferences defaultPreferences = new()
        {
            price_per_km = new PreferenceEntryDef() { id = PreferenceFields.PricePerKm, name = "Price per km", description = "The amount of cash/online balance to pay for the service per kilometer.", value = 13, type = typeof(int) },
            use_cash = new PreferenceEntryDef() { id = PreferenceFields.UseCash, name = "Use Cash", description = "Use cash instead of online balance. (true/false)", value = false, type = typeof(bool) },
            service_charge_day = new PreferenceEntryDef() { id = PreferenceFields.ServiceChargeDay, name = "Service Charge (Day)", description = "Flat fee for the service during the day.", value = 500, type = typeof(int) },
            service_charge_night = new PreferenceEntryDef() { id = PreferenceFields.ServiceChargeNight, name = "Service Charge (Night)", description = "Flat fee for the service during the night.", value = 800, type = typeof(int) },
            bypass_checkpoints = new PreferenceEntryDef() { id = PreferenceFields.BypassCheckpoints, name = "Bypass Checkpoints", description = "Should this mod bypasses the checkpoints?", value = true, type = typeof(bool) },
            app_price = new PreferenceEntryDef() { id = PreferenceFields.AppPrice, name = "App Price", description = "The price of the app from Marco.", value = 5000, type = typeof(int) }
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
                category = MelonPreferences.CreateCategory(ConfigCategoryName, "Call Vehicle Preferences");
            }

            // Updated to use the recommended MelonEnvironment.UserDataDirectory instead of the obsolete MelonUtils.UserDataDirectory
            string filePath = Path.Combine(MelonEnvironment.UserDataDirectory, ConfigFileName);

            // Set the file path for the category, disable auto-loading for manual control
            category.SetFilePath(filePath, autoload: false);

            bool fileNeedsSaving = false;

            // Check if the configuration file exists
            if (!File.Exists(filePath))
            {
                CreateDefaultEntries(); // Create all entries based on defaults
                fileNeedsSaving = true; // Mark that the new file needs to be saved
            }
            else
            {
                category.LoadFromFile(false); // Load existing file without saving immediately

                // Check if existing file has all necessary entries, add missing ones
                if (CheckAndCreateMissingEntries())
                {
                    fileNeedsSaving = true; // Mark that the updated file needs saving
                }
            }

            // Save the file only if new entries were created (either new file or added missing)
            if (fileNeedsSaving)
            {
                category.SaveToFile(false); // Save changes without reloading
            }
            Logger.Msg($"'{ConfigCategoryName}' preferences setup complete.");
        }

        /// <summary>
        /// Creates all preference entries based on the 'defaultPreferences' structure.
        /// Assumes the category is clean or entries don't exist.
        /// </summary>
        private static void CreateDefaultEntries()
        {
            if (category == null) return;
            FieldInfo[] properties = typeof(DefaultPreferences).GetFields();
            foreach (var prop in properties)
            {
                var entryValue = prop.GetValue(defaultPreferences);
                if (entryValue is PreferenceEntryDef preference)
                {
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

            foreach (var prop in properties)
            {
                var entryValue = prop.GetValue(defaultPreferences);
                if (entryValue is PreferenceEntryDef preference)
                {
                    if (!category.HasEntry(preference.id))
                    {
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
                return default(T);
            }

            if (!category.HasEntry(key))
            {
                // Optionally, find and return the configured default value from defaultPreferences
                return GetDefaultValue<T>(key);
            }

            var entry = category.GetEntry<T>(key); // Use generic GetEntry for type safety
            if (entry == null)
            {
                // This might happen if the type T doesn't match the stored type
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
                return false;
            }

            if (!category.HasEntry(key))
            {
                return false;
            }

            var entry = category.GetEntry<T>(key);
            if (entry == null)
            {
                // Optionally try GetEntry(key).BoxedValue = value; but it's less safe
                return false;
            }

            try
            {
                entry.Value = value;
                category.SaveToFile(false); // Save immediately after setting
                return true;
            }
            catch (Exception ex)
            {
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
                        return default(T); // Return default(T) on conversion error
                    }
                }
            }
            return default(T); // Return default(T) if key not found in defaults
        }
    }
}