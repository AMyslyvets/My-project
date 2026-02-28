using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lesson8
{
    public class TreeBuilder : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Transform _trunk;
        [SerializeField] private Transform _crown;

        private Mesh _treeMesh;

        [ContextMenu("Build Tree")]
        private void BuildTree()
        {
            ClearTree();

            _treeMesh = new Mesh();

            Matrix4x4 matrix = transform.worldToLocalMatrix;

            CombineInstance[] parts = new CombineInstance[2];
            Mesh trunkMesh = BuildPart(_trunk);
            Mesh crownMesh = BuildPart(_crown);

            parts[0].mesh = trunkMesh;
            parts[0].transform = matrix * _trunk.localToWorldMatrix;

            parts[1].mesh = crownMesh;
            parts[1].transform = matrix * _crown.localToWorldMatrix;

            _treeMesh.CombineMeshes(parts, false, true);
            _treeMesh.name = gameObject.name;
            _meshFilter.sharedMesh = _treeMesh;
        }

        [ContextMenu("Clear Tree")]
        private void ClearTree()
        {
            if (_treeMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_treeMesh);
                else
                    DestroyImmediate(_treeMesh);
            }
        }

        [ContextMenu("Save Tree")]
        private void SaveTree()
        {
#if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanelInProject("Save Tree", gameObject.name, "asset", "Save Tree");
            AssetDatabase.CreateAsset(_treeMesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        private void OnDestroy()
        {
            if (_treeMesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_treeMesh);
                else
                    DestroyImmediate(_treeMesh);
            }
        }

        private Mesh BuildPart(Transform part)
        {
            Mesh mesh = new Mesh();

            MeshFilter[] meshFilters = part.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];

            for (int i = 0; i < combineInstances.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                combineInstances[i].mesh = meshFilter.sharedMesh;
                combineInstances[i].transform = part.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
            }

            mesh.CombineMeshes(combineInstances, true, true);

            return mesh;
        }
    }
}