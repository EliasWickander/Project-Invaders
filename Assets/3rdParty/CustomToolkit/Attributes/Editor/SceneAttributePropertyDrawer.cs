using UnityEditor;
using UnityEngine;

namespace CustomToolkit.Attributes
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Unknown type. Can only use [Scene] attribute on strings");
                return;
            }
            
            SceneAsset sceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

            if (sceneObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
            {
                //If scene is not found at path, check in build settings if scene used to have that path
                sceneObject = GetSceneInBuildSettings(property.stringValue);
            }

            if (sceneObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
            {
                Debug.LogError($"Could not find scene {property.stringValue} in {property.propertyPath}");
            }

            SceneAsset scene = (SceneAsset) EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), true);

            property.stringValue = AssetDatabase.GetAssetPath(scene);
        }

        private SceneAsset GetSceneInBuildSettings(string sceneName)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset != null && sceneAsset.name == sceneName)
                {
                    return sceneAsset;
                }
            }

            return null;
        }
    }
}