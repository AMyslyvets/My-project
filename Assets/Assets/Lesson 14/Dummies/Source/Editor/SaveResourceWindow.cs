using UnityEditor;
using UnityEngine;

namespace Dummies.Editor
{
    public class SaveResourceWindow : EditorWindow
    {
        [MenuItem("Tools/Save Resource Window")]
        public static void ShowWindow()
        {
            GetWindow(typeof(SaveResourceWindow), false, "Save Resource Window").Show();
        }
        
        private UnityEditor.Editor _selfEditor;

        private void OnEnable()
        {
            _selfEditor = UnityEditor.Editor.CreateEditor(this);
        }

        private void OnDisable()
        {
            DestroyImmediate(_selfEditor);
        }

        private void OnGUI()
        {
            _selfEditor.DrawDefaultInspector();
            if (GUILayout.Button("Save Selection"))
                SaveSelection();
        }

        private void SaveSelection()
        {
            Debug.Log($"Selection: {(Selection.activeObject == null ? "NULL" : Selection.activeObject.name)}");
            if ((Selection.activeObject is Sprite sprite))
                SaveSprite(sprite);
        }

        private void SaveSprite(Sprite sprite)
        {
            string path = EditorUtility.SaveFilePanel("Save Selection", Application.dataPath, sprite.name, "png");
            Debug.Log(path);
            if (string.IsNullOrWhiteSpace(path))
                return;
            Texture2D texture = new Texture2D(sprite.texture.width, sprite.texture.height);
            Graphics.CopyTexture(sprite.texture, texture);
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}