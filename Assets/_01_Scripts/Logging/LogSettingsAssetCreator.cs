#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Logging.Editor
{
    public static class LogSettingsAssetCreator
    {
        private const string ResourcesDir = "Assets/Resources";
        private const string AssetPath = "Assets/Resources/LogSettings.asset";

        [MenuItem("Tools/Logging/Create LogSettings Asset")]
        public static void CreateAsset()
        {
            if (!Directory.Exists(ResourcesDir))
                Directory.CreateDirectory(ResourcesDir);

            var existing = AssetDatabase.LoadAssetAtPath<LogSettings>(AssetPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return;
            }

            var asset = ScriptableObject.CreateInstance<LogSettings>();
            AssetDatabase.CreateAsset(asset, AssetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
#endif
