using FullSerializer;
using UnityEngine;

namespace PuzzleGame.Core
{
    public class FileUtils : MonoBehaviour
    {
        public static T LoadJson<T>(fsSerializer fs, string path) where T : class
        {
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"Failed to load JSON file at path: {path}");
                return null;
            }
            var json = textAsset.text;
            var data = fsJsonParser.Parse(json);
            object deserialized = null;
            var result = fs.TryDeserialize(data, typeof(T), ref deserialized);
            if (!result.Succeeded)
            {
                Debug.LogError($"Failed to deserialize JSON file at path: {path}. Error: {result.FormattedMessages}");
                return null;
            }
            return deserialized as T;
        }

        public static bool FileExists(string path)
        {
            var textAsset = Resources.Load<TextAsset>(path);
            return textAsset != null;
        }
    }
}
