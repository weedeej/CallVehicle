using MelonLoader;
using MelonLoader.Utils;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.Persistence;
using UnityEngine;
using ScheduleOne.DevUtilities;
using System.Reflection;

namespace CallVehicle.Utilities
{
    public partial struct CallVehicleAppSaveData
    {
        public bool isPurchased;
    }
    public static class ModUtilities
    {

        public static Texture2D LoadCustomImage(string fileName)
        {
            string path = Path.Combine(MelonEnvironment.UserDataDirectory, fileName);
            if (!File.Exists(path)) return null;
            byte[] array = File.ReadAllBytes(path);
            Texture2D texture2D = new Texture2D(2, 2);
            ImageConversion.LoadImage(texture2D, array);
            return texture2D;
        }

        public static Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static Sprite SpriteFromImage(string fileName)
        {
            Texture2D texture = LoadCustomImage(fileName);
            if (texture == null) return null;
            return CreateSprite(texture);
        }

        public static string GetFullSavePath()
        {
            new WaitUntil(() => Singleton<LoadManager>.Instance.LoadedGameFolderPath != null);
            string savePath = Singleton<LoadManager>.Instance.LoadedGameFolderPath;
            string marker = @"\Saves\";
            int markerIndex = savePath.IndexOf(marker);
            if (markerIndex == -1) return null;
            int startIndex = markerIndex + marker.Length;
            savePath = Path.Combine(MelonEnvironment.UserDataDirectory, @$"Call Vehicle Data\{savePath.Substring(startIndex)}");
            // Check if the directory exists, if not create it
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            return Path.Combine(savePath, "CallVehicleAppStatus.json");
        }

        public static CallVehicleAppSaveData? GetLatestSaveData()
        {
            string fullSavePath = GetFullSavePath();
            VariablesLoader loader = new VariablesLoader();
            string content;
            bool isLoaded = loader.TryLoadFile(fullSavePath, out content, false);
            if (isLoaded)
            {
                CallVehicleAppSaveData app = JsonUtility.FromJson<CallVehicleAppSaveData>(content);
                return app;
            }
            return null;
        }

        public static CallVehicleAppSaveData SaveModData(CallVehicleAppSaveData data)
        {
            string fullSavePath = GetFullSavePath();
            File.WriteAllText(fullSavePath, JsonUtility.ToJson(data));
            return data;
        }
    }
}
