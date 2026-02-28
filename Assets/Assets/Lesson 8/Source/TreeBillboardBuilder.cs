using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Lesson8
{
    public class TreeBillboardBuilder : MonoBehaviour
    {
        [SerializeField, TextArea] private string _info;
        [SerializeField] private BillboardAsset _billboardAsset;
        [SerializeField] private Vector2[] _vertices;
        [SerializeField] private ushort[] _indices;
        [SerializeField] private Vector4[] _imageCoords;

        [ContextMenu("Test")]
        public void Test()
        {
            #if UNITY_EDITOR
            _billboardAsset = new BillboardAsset();
            _billboardAsset.SetVertices(new[]
                { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) });
            _billboardAsset.SetIndices(new ushort[] { 0, 1, 2, 0, 3, 2 });

            StringBuilder stringBuilder = new StringBuilder("Billboard:");
            stringBuilder.AppendLine($"Index count: {_billboardAsset.indexCount}");
            stringBuilder.AppendLine($"Vertex count: {_billboardAsset.vertexCount}");
            stringBuilder.AppendLine($"Image count: {_billboardAsset.imageCount}");

            _info = stringBuilder.ToString();

            string path = EditorUtility.SaveFilePanelInProject("Save Billboard Asset", "Tree billboard", "asset",
                "Save Billboard Asset");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(_billboardAsset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            #endif
        }

        [ContextMenu("Set geometry")]
        private void SetGeometry()
        {
            _billboardAsset.SetVertices(_vertices);
            _billboardAsset.SetIndices(_indices);
            _billboardAsset.SetImageTexCoords(_imageCoords);
        }
    }
}