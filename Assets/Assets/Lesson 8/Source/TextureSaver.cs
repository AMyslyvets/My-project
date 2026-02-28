using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lesson8
{
    public class TextureSaver : MonoBehaviour
    {
        [SerializeField] private Texture2D _texture;
        
        [ContextMenu("Save Texture")]
        public void SaveTexture()
        {
#if UNITY_EDITOR
            Texture2D texture = Instantiate(_texture);
            
            string path = EditorUtility.SaveFilePanelInProject("Save Texture Asset", texture.name, "asset",
                "Save Texture Asset");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(texture, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif
        }
    }
}