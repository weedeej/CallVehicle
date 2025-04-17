using FluffyUnderware.DevTools.Extensions;
using MelonLoader;
public static class PreferenceFields
{
    public static string ServicePrice = "service_price";
    public static string UseCash = "use_cash";
}
namespace CallVehicle
{
    public static class Preferences
    {
        public struct DefaultPreferences
        {
            public PreferenceEntry service_price;
            public PreferenceEntry use_cash;
        }

        public struct PreferenceEntry
        {
            public string id;
            public string name;
            public string description;
            public object value;
        }

        public static readonly DefaultPreferences defaultPreferences = new() {
            service_price = new PreferenceEntry() { id = "service_price", name = "Price for the service", description = "The amount of cash/online balance to pay for the service", value = 5000 },
            use_cash = new PreferenceEntry() { id = "use_cash", name = "Use cash for transactions", description = "Use cash instead of online balance. default: online balance", value = false }
        };

        private static MelonPreferences_Category category = MelonPreferences.GetCategory("CallVehicle") ??
            MelonPreferences.CreateCategory("CallVehicle", "Call Vehicle Preferences");
        public static void Initialize()
        {
            category.SetFilePath("UserData/ChauffeurPrefs.cfg");
            category.LoadFromFile();

            System.Reflection.FieldInfo[] properties = typeof(DefaultPreferences).GetFields();
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
    }
}
