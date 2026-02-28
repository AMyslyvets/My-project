using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lesson8
{
    public class TreePrefabBuilder : MonoBehaviour
    {
        [Serializable]
        public class Bone
        {
            public Transform transform;
            public Transform rendererGroup;
            public float height;
            [SerializeReference] public List<Bone> children = new();

            [HideInInspector][SerializeField] protected TreePrefabBuilder builder;
            [HideInInspector][SerializeReference] protected Bone parent;
            
            public Bone Parent => parent;
            public TreePrefabBuilder Builder => builder;
            
            public Bone(TreePrefabBuilder builder, Bone parent, string name)
            {
                Initialize(builder, parent, name);
            }

            public Bone(TreePrefabBuilder builder, Bone parent, string name, float height)
            {
                this.height = height;
                Initialize(builder, parent, name);
            }
            
            public Bone(TreePrefabBuilder builder, Vector3 position, Quaternion rotation)
            {
                this.builder = builder;
                transform = Instantiate(this.builder._bone, this.builder._base);
                transform.SetLocalPositionAndRotation(position, rotation);
                transform.gameObject.SetActive(true);
                transform.gameObject.name = "Root";
            }

            private void Initialize(TreePrefabBuilder builder, Bone parent, string name)
            {
                this.parent = parent;
                this.builder = builder;
                transform = Instantiate(this.builder._bone, parent.transform);
                transform.localPosition = Vector3.forward * height;
                transform.gameObject.SetActive(true);

                int level = 1;
                Bone iterator = parent;
                while (iterator.parent != null)
                {
                    level++;
                    iterator = iterator.parent;
                }
                
                transform.gameObject.name = $"{name}-{level} ({this.parent.children.Count})";
            }

            public void RemoveSelf()
            {
                if (parent == null)
                    return;
                parent.children.Remove(this);
                DestroyImmediate(transform.gameObject);
            }

            public void SetHeight(float height)
            {
                this.height = height;
                transform.localPosition = Vector3.forward * height;
            }

            public virtual void RegenerateRenderer()
            {
                if (rendererGroup != null)
                {
                    DestroyImmediate(rendererGroup.gameObject);
                }

                for (int i = 0; i < children.Count; i++)
                {
                    children[i].RegenerateRenderer();
                }
            }
        }

        [Serializable]
        public class Branch : Bone
        {
            public float length;
            public float radius;
            public float inclination;
            public float axialRoll;

            public Branch(TreePrefabBuilder builder, Vector3 position, Quaternion rotation, float length, float radius, float inclination, float axialRoll) :
                base(builder, position, rotation)
            {
                this.length = length;
                this.inclination = inclination;
                this.axialRoll = axialRoll;
                this.radius = radius;
                ApplyRotation();
                InitializeBranch();
            }

            public Branch(TreePrefabBuilder builder, Bone parent, float height, float length, float radius, float inclination, float axialRoll) : base(builder, parent,
                "Branch", height)
            {
                this.length = length;
                this.radius = radius;
                this.inclination = inclination;
                this.axialRoll = axialRoll;
                ApplyRotation();
                InitializeBranch();
            }
            
            public void AddBranch()
            {
                children.Add(new Branch(builder, this, length * 0.5f, length * 0.67f, radius * 0.67f, 30f, Random.Range(0f, 360f)));
            }
            
            public void AddLeaf()
            {
                children.Add(new Leaf(builder, this, length * 1.15f, length * 0.33f));
            }

            public void SetLength(float value)
            {
                length = value;
                rendererGroup.SetLocalPositionAndRotation(Vector3.forward * (length * 0.5f), Quaternion.identity);
                float rendererHeight = length * 0.5f;
                rendererGroup.localScale = new Vector3(radius, radius, rendererHeight);
            }
            
            public void SetRadius(float value)
            {
                radius = value;
                float rendererHeight = length * 0.5f;
                rendererGroup.localScale = new Vector3(radius, radius, rendererHeight);
            }
            
            public void SetInclination(float value)
            {
                inclination = value;
                ApplyRotation();
            }

            public void SetAxialRoll(float value)
            {
                axialRoll = value;
                ApplyRotation();
            }

            private void ApplyRotation()
            {
                if (parent == null)
                    return;
                
                Quaternion rotation = Quaternion.AngleAxis(axialRoll, Vector3.forward) * Quaternion.AngleAxis(inclination, Vector3.right);
                transform.localRotation = rotation;
            }

            private void InitializeBranch()
            {
                rendererGroup = Instantiate(builder._branchPrefab, transform);
                rendererGroup.SetLocalPositionAndRotation(Vector3.forward * (length * 0.5f), Quaternion.identity);
                float rendererHeight = length * 0.5f;
                rendererGroup.localScale = new Vector3(radius, radius, rendererHeight);
            }

            public override void RegenerateRenderer()
            {
                base.RegenerateRenderer();
                InitializeBranch();
            }
        }
        
        public class Leaf : Bone
        {
            public float size;

            public Leaf(TreePrefabBuilder builder, Bone parent, float size) : base(builder, parent, "Leaf")
            {
                this.size = size;
                InitializeLeaf();
            }

            public Leaf(TreePrefabBuilder builder, Bone parent, float height, float size) : base(builder, parent, "Leaf", height)
            {
                this.size = size;
                InitializeLeaf();
            }

            public Leaf(TreePrefabBuilder builder, Vector3 position, Quaternion rotation, float size) : base(builder, position, rotation)
            {
                this.size = size;
                InitializeLeaf();
            }

            private void InitializeLeaf()
            {
                rendererGroup = Instantiate(builder._leafPrefab, transform);
                rendererGroup.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                rendererGroup.localScale = Vector3.one * size;
            }

            public void SetSize(float value)
            {
                size = value;
                rendererGroup.localScale = Vector3.one * size;
            }
            
            public override void RegenerateRenderer()
            {
                base.RegenerateRenderer();
                InitializeLeaf();
            }
        }

        [SerializeField] private Transform _bone;
        [SerializeField] private Transform _base;
        [SerializeField] private Transform _branchPrefab;
        [SerializeField] private Transform _leafPrefab;
        [HideInInspector][SerializeReference] public Bone root;

        private Mesh _treeMesh;
        
        private void OnDrawGizmos()
        {
            if (root == null)
                return;
            ProcessBone(root);
        }

        private void ProcessBone(Bone bone)
        {
            DrawBone(bone);
            for (int i = 0; i < bone.children.Count; ++i)
            {
                ProcessBone(bone.children[i]);
            }
        }
        
        private void DrawBone(Bone bone)
        {
            if (bone.transform == null)
                return;
            Gizmos.color = Color.green;
            
            if (bone is Branch branch)
                DrawBranch(branch);
            if (bone is Leaf leaf)
                DrawLeaf(leaf);
        }

        private void DrawBranch(Branch branch)
        {
            Gizmos.DrawLine(branch.transform.position, branch.transform.position + branch.transform.forward * branch.length);
        }
        
        private void DrawLeaf(Leaf leaf)
        {
            Gizmos.DrawWireSphere(leaf.transform.position, leaf.size);
        }

        [ContextMenu("Create Tree")]
        public void CreateTree()
        {
            root = new Branch(this, Vector3.zero, Quaternion.identity, 5f, 1f, 0f, 0f);
        }
        
        [ContextMenu("Delete Tree")]
        public void DeleteTree()
        {
            DestroyImmediate(root.transform.gameObject);
            root = null;
            GC.Collect();
        }

        [ContextMenu("Update Tree")]
        public void UpdateTree()
        {
            root.RegenerateRenderer();
        }

        [ContextMenu("Align Leafs")]
        public void AlignLeafs()
        {
            AlignNode(root);
        }

        public void AlignNode(Bone node)
        {
            if (node is Leaf leaf)
            {
                node.transform.rotation = Quaternion.identity;
            }

            for (int i = 0; i < node.children.Count; ++i)
            {
                AlignNode(node.children[i]);
            }
        }
        
        [ContextMenu("Build Tree")]
        private void BuildTree()
        {
            ClearTree();

            _treeMesh = new Mesh();

            Matrix4x4 matrix = transform.worldToLocalMatrix;

            CombineInstance[] parts = new CombineInstance[2];
            Mesh trunkMesh = BuildPartBranches(transform, GetBranches().ToArray());
            Mesh crownMesh = BuildPartLeaves(transform, GetLeafs().ToArray());

            parts[0].mesh = trunkMesh;
            parts[0].transform = matrix * transform.localToWorldMatrix;

            parts[1].mesh = crownMesh;
            parts[1].transform = matrix * transform.localToWorldMatrix;

            _treeMesh.CombineMeshes(parts, false, true);
            _treeMesh.name = gameObject.name;
        }

        private List<Leaf> GetLeafs()
        {
            List<Leaf> leafs = new List<Leaf>();
            FillLeafs(root, leafs);
            return leafs;
        }

        private void FillLeafs(Bone node, List<Leaf> leafs)
        {
            if (node is Leaf leaf)
            {
                leafs.Add(leaf);
            }
            
            for (int i = 0; i < node.children.Count; ++i)
                FillLeafs(node.children[i], leafs);
        }
        
        private List<Branch> GetBranches()
        {
            List<Branch> branches = new List<Branch>();
            FillBranches(root, branches);
            return branches;
        }

        private void FillBranches(Bone node, List<Branch> branches)
        {
            if (node is Branch branch)
            {
                branches.Add(branch);
            }
            
            for (int i = 0; i < node.children.Count; ++i)
                FillBranches(node.children[i], branches);
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

        private Mesh BuildPartBranches(Transform part, Branch[] branches)
        {
            Mesh mesh = new Mesh();

            List<CombineInstance> combineInstances = new();

            for (int i = 0; i < branches.Length; ++i)
            {
                Branch branch = branches[i]; 
                float uScale = branch.radius;
                float vScale = branch.length * 0.5f;
                
                MeshFilter[] meshFilters = branch.rendererGroup.GetComponentsInChildren<MeshFilter>();

                for (int j = 0; j < meshFilters.Length; ++j)
                {
                    MeshFilter meshFilter = meshFilters[j];
                    Mesh instanceMesh = Instantiate(meshFilter.sharedMesh);
                    Vector2[] uvs = instanceMesh.uv;
                    for (int k = 0; k < uvs.Length; ++k)
                    {
                        Vector2 uv = uvs[k];
                        uv.x *= uScale;
                        uv.y *= vScale;
                        uvs[k] = uv;
                    }
                    instanceMesh.uv = uvs;
                    CombineInstance combineInstance = new CombineInstance
                    {
                        mesh = instanceMesh,
                        transform = part.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix
                    };
                    combineInstances.Add(combineInstance);
                }
            }

            mesh.CombineMeshes(combineInstances.ToArray(), true, true);

            return mesh;
        }
        
        private Mesh BuildPartLeaves(Transform part, Leaf[] leaves)
        {
            Mesh mesh = new Mesh();

            List<CombineInstance> combineInstances = new();

            for (int i = 0; i < leaves.Length; i++)
            {
                Leaf leaf = leaves[i]; 
                float uvScale = leaf.size;
                
                MeshFilter[] meshFilters = leaf.rendererGroup.GetComponentsInChildren<MeshFilter>();

                for (int j = 0; j < meshFilters.Length; ++j)
                {
                    MeshFilter meshFilter = meshFilters[j];
                    Mesh instanceMesh = Instantiate(meshFilter.sharedMesh);
                    Vector2[] uvs = instanceMesh.uv;
                    for (int k = 0; k < uvs.Length; ++k)
                    {
                        uvs[k] *= uvScale;
                    }
                    instanceMesh.uv = uvs;
                    CombineInstance combineInstance = new CombineInstance
                    {
                        mesh = instanceMesh,
                        transform = part.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix
                    };
                    combineInstances.Add(combineInstance);
                }
            }

            mesh.CombineMeshes(combineInstances.ToArray(), true, true);

            return mesh;
        }
    }
}