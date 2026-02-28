using System;
using UnityEditor;
using UnityEngine;

namespace PhysX
{
    [Serializable]
    public struct MeshGroup
    {
        public Transform root;
        public MeshFilter[] meshFilters;
    }
    
    public class MeshCombiner : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshGroup[] _meshGroups;

        private Mesh _mesh;
        
        [ContextMenu("Combine Meshes")]
        private void CombineMeshes()
        {
            ClearMesh();
    
            _mesh = new Mesh();
            
            Matrix4x4 matrix = transform.worldToLocalMatrix;
            
            CombineInstance[] groups = new CombineInstance[_meshGroups.Length];

            for (int i = 0; i < _meshGroups.Length; ++i)
            {
                groups[i].mesh = BuildGroup(_meshGroups[i]);
                groups[i].transform = matrix * _meshGroups[i].root.localToWorldMatrix;
            }
            
            _mesh.CombineMeshes(groups, false, true);
            _mesh.name = gameObject.name;
            _meshFilter.sharedMesh = _mesh;
        }
    
        [ContextMenu("Clear Mesh")]
        private void ClearMesh()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_mesh);
                else
                    DestroyImmediate(_mesh);
            }
        }
    
        [ContextMenu("Save Mesh")]
        private void SaveMesh()
        {
            #if UNITY_EDITOR
            string path = EditorUtility.SaveFilePanelInProject("Save Tree", gameObject.name, "asset", "Save Tree");
            AssetDatabase.CreateAsset(_mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            #endif
        }
        
        private void OnDestroy()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_mesh);
                else
                    DestroyImmediate(_mesh);
            }
        }
        
        private Mesh BuildGroup(MeshGroup group)
        {
            Mesh mesh = new Mesh();

            CombineInstance[] combineInstances = new CombineInstance[group.meshFilters.Length];

            for (int i = 0; i < combineInstances.Length; i++)
            {
                MeshFilter meshFilter = group.meshFilters[i];
                combineInstances[i].mesh = meshFilter.sharedMesh;
                combineInstances[i].transform = group.root.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
            }
        
            mesh.CombineMeshes(combineInstances, true, true);

            return mesh;
        }
    }
}