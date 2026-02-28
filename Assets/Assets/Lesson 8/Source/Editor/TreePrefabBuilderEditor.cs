using Codice.CM.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lesson8.Editor
{
    [CustomEditor(typeof(TreePrefabBuilder))]
    public class TreePrefabBuilderEditor : UnityEditor.Editor
    {
        private Button _button;
        private VisualElement _root;
        private Box _box;
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            InspectorElement.FillDefaultInspector(_root, serializedObject, null);
            _box = new Box();
            CreateTreeGUI();
            _root.Add(_box);
            return _root;
        }

        private void CreateTreeGUI()
        {
            _box.Clear();
            
            TreePrefabBuilder builder = target as TreePrefabBuilder;
            EditorUtility.SetDirty(builder);
            if (builder?.root == null)
            {
                _button = new Button(CreateTree)
                {
                    text = "Create Tree"
                };
            }
            else
            {
                Button updateButton = new Button(UpdateTree)
                {
                    text = "Update Tree Renderers"
                };
                
                _box.Add(updateButton);
                
                _button = new Button(DeleteTree)
                {
                    text = "Delete Tree"
                };

                string msg = "[CreateInspectorGUI] Root children";
                for (int i = 0; i < builder.root.children.Count; ++i)
                {
                    msg += $"\n[{i}]: '{builder.root.children[i].transform?.name ?? "[NULL]"}'";
                }
                Debug.Log(msg);
                
                _box.Add(CreateBranchGUI(builder.root as TreePrefabBuilder.Branch));
            }

            _box.Add(_button);
        }

        private void UpdateTree()
        {
            TreePrefabBuilder builder = target as TreePrefabBuilder;
            builder?.UpdateTree();
        }
        
        private void DeleteTree()
        {
            TreePrefabBuilder builder = target as TreePrefabBuilder;
            builder?.DeleteTree();
            CreateTreeGUI();
        }
        
        private void CreateTree()
        {
            TreePrefabBuilder builder = target as TreePrefabBuilder;
            builder?.CreateTree();
            CreateTreeGUI();
        }

        private void AddBranch(TreePrefabBuilder.Branch branch)
        {
            branch.AddBranch();
            CreateTreeGUI();
        }
        
        private void AddLeaf(TreePrefabBuilder.Branch branch)
        {
            branch.AddLeaf();
            CreateTreeGUI();
        }

        private void RemoveNode(TreePrefabBuilder.Bone bone)
        {
            bone.RemoveSelf();
            CreateTreeGUI();
        }
        
        public VisualElement CreateBoneGUI(TreePrefabBuilder.Bone bone)
        {
            Box box = new Box();
            box.style.backgroundColor = new StyleColor(new Color(.15f, .15f, .15f, .75f));
            StyleColor borderColor = new StyleColor(Color.black);
            box.style.borderBottomColor = box.style.borderLeftColor =
                box.style.borderRightColor = box.style.borderTopColor = borderColor;
            box.style.borderLeftWidth = box.style.borderTopWidth =
                box.style.borderRightWidth = box.style.borderBottomWidth= 2;
            box.style.paddingLeft = 10;
            box.style.marginBottom = 5;
            
            box.Add(new Label(bone?.transform.name ?? "[NULL]"));

            if (bone == null)
                return box;
            
            if (bone.Parent != null)
            {
                Button removeButton = new Button(() => RemoveNode(bone))
                {
                    text = "Remove Node"
                };
                box.Add(removeButton);
                
                FloatField heightField = new FloatField("Height")
                {
                    value = bone.height
                };
                heightField.RegisterValueChangedCallback(evt => HeightHandler(bone, evt));
                box.Add(heightField);
            }
            
            return box;
        }

        public void FillChildren(VisualElement visualElement, TreePrefabBuilder.Bone bone)
        {
            visualElement.Add(new Label("Children"));
            
            for (int i = 0; i < bone.children.Count; ++i)
            {
                TreePrefabBuilder.Bone entry = bone.children[i];
                VisualElement boneElement = entry switch
                {
                    TreePrefabBuilder.Branch branch => CreateBranchGUI(branch),
                    TreePrefabBuilder.Leaf leaf => CreateLeafGUI(leaf),
                    _ => CreateBoneGUI(entry),
                };
                
                visualElement.Add(boneElement);
            }
        }
        
        public VisualElement CreateBranchGUI(TreePrefabBuilder.Branch branch)
        {
            VisualElement container = CreateBoneGUI(branch);

            if (branch == null)
                return container;
            
            FloatField lengthField = new FloatField("Length")
            {
                value = branch.length
            };
            lengthField.RegisterValueChangedCallback(evt => LengthHandler(branch, evt));
            container.Add(lengthField);

            FloatField radiusField = new FloatField("Radius")
            {
                value = branch.radius
            };
            radiusField.RegisterValueChangedCallback(evt => RadiusHandler(branch, evt));
            container.Add(radiusField);
            
            if (branch.Parent != null)
            {
                FloatField inclinationField = new FloatField("Inclination")
                {
                    value = branch.inclination
                };
                inclinationField.RegisterValueChangedCallback(evt => InclinationHandler(branch, evt));
                container.Add(inclinationField);

                FloatField axialRollField = new FloatField("Axial Roll")
                {
                    value = branch.axialRoll
                };
                axialRollField.RegisterValueChangedCallback(evt => AxialRollHandler(branch, evt));
                container.Add(axialRollField);
            }

            FillChildren(container, branch);
            
            Button branchButton = new Button(() => AddBranch(branch))
            {
                text = "Add Branch"
            };
            container.Add(branchButton);
            Button leafButton = new Button(() => AddLeaf(branch))
            {
                text = "Add Leaf"
            };
            container.Add(leafButton);
            return container;
        }
        
        public VisualElement CreateLeafGUI(TreePrefabBuilder.Leaf leaf)
        {
            VisualElement container = CreateBoneGUI(leaf);

            if (leaf == null)
                return container;
            
            FloatField sizeField = new FloatField("Size")
            {
                value = leaf.size
            };
            sizeField.RegisterValueChangedCallback(evt => SizeHandler(leaf, evt));
            container.Add(sizeField);
            
            return container;
        }

        private void HeightHandler(TreePrefabBuilder.Bone bone, ChangeEvent<float> evt)
        {
            bone.SetHeight(evt.newValue);
            EditorUtility.SetDirty(bone.Builder);
        }
        
        private void LengthHandler(TreePrefabBuilder.Branch branch, ChangeEvent<float> evt)
        {
            branch.SetLength(evt.newValue);
            EditorUtility.SetDirty(branch.Builder);
        }
        
        private void InclinationHandler(TreePrefabBuilder.Branch branch, ChangeEvent<float> evt)
        {
            branch.SetInclination(evt.newValue);
            EditorUtility.SetDirty(branch.Builder);
        }
        
        private void RadiusHandler(TreePrefabBuilder.Branch branch, ChangeEvent<float> evt)
        {
            branch.SetRadius(evt.newValue);
            EditorUtility.SetDirty(branch.Builder);
        }
        
        private void SizeHandler(TreePrefabBuilder.Leaf leaf, ChangeEvent<float> evt)
        {
            leaf.SetSize(evt.newValue);
            EditorUtility.SetDirty(leaf.Builder);
        }
        
        private void AxialRollHandler(TreePrefabBuilder.Branch branch, ChangeEvent<float> evt)
        {
            branch.SetAxialRoll(evt.newValue);
            EditorUtility.SetDirty(branch.Builder);
        }
    }
}