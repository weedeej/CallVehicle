using FluffyUnderware.DevTools.Extensions;
using MelonLoader;
public static class PreferenceFields
{
    public static string PricePerKm = "price_per_km";
    public static string UseCash = "use_cash";
    public static string ServiceChargeDay = "service_charge_day";
    public static string ServiceChargeNight = "service_charge_night";
}
namespace CallVehicle
{
    public static class Preferences
    {
        public struct DefaultPreferences
        {
            public PreferenceEntry price_per_km;
            public PreferenceEntry use_cash;
            public PreferenceEntry service_charge_day;
            public PreferenceEntry service_charge_night;
        }

        public struct PreferenceEntry
        {
            public string id;
            public string name;
            public string description;
            public object value;
        }

        public static readonly DefaultPreferences defaultPreferences = new() {
            price_per_km = new PreferenceEntry() { id = "price_per_km", name = "Price for the service per km", description = "The amount of cash/online balance to pay for the service", value = 13 },
            use_cash = new PreferenceEntry() { id = "use_cash", name = "Use cash for transactions", description = "Use cash instead of online balance. default: online balance", value = false },
            service_charge_day = new PreferenceEntry() { id = "service_charge_day", name = "Service charge during the day", description = "The amount of cash/online balance to pay for the service during the day", value = 500 },
            service_charge_night = new PreferenceEntry() { id = "service_charge_night", name = "Service charge during the night", description = "The amount of cash/online balance to pay for the service during the night", value = 800 }
        };

        private static MelonPreferences_Category category = MelonPreferences.GetCategory("CallVehicle") ??
            MelonPreferences.CreateCategory("CallVehicle", "Call Vehicle Preferences");
        public static void Initialize()
        {
            category.SetFilePath("UserData/ChauffeurPrefs.cfg");
            category.LoadFromFile();

            System.Reflection.FieldInfo[] properties = typeof(DefaultPreferences).GetFields();
            // Check if same count of entries against properties. And if ids are the same.
            if (category.Entries.Count < properties.Length)
            {
                properties.ForEach(prop =>
                {
                    var entry = prop.GetValue(defaultPreferences);
                    if (entry is PreferenceEntry preference && !category.HasEntry(preference.id))
                        category.CreateEntry(preference.id, preference.value, preference.name, preference.description);
                });
            }
            category.SaveToFile(false);
            category.LoadFromFile();
        }

        public static T GetPrefValue<T>(string key)
        {
            return (T)(category.HasEntry(key) ? category.GetEntry(key).BoxedValue : null);
        }

        public static bool SetPrefValue(string key, object value)
        {
            if (!category.HasEntry(key)) return false;
            var entry = category.GetEntry(key);
            entry.BoxedValue = value;
            MelonPreferences.Save();
            return true;
        }
    }
}
