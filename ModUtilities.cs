using MelonLoader.Utils;
using UnityEngine;

namespace CallVehicle.Utilities
{
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
    }
}
