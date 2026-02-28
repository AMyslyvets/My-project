using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Lesson8.Editor
{
    /*[CustomPropertyDrawer(typeof(TreePrefabBuilder.Branch))]
    public class BranchPropertyDrawer : BonePropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            TreePrefabBuilder.Branch branch = property.managedReferenceValue as TreePrefabBuilder.Branch;
            
            VisualElement container = base.CreatePropertyGUI(property);

            if (branch == null)
                return container;
            
            Button branchButton = new Button(() => OnAddBranchClickedHandler(branch, container, property))
            {
                text = "Add Branch"
            };
            container.Add(branchButton);
            Button leafButton = new Button(() => OnAddLeafClickedHandler(branch, container))
            {
                text = "Add Leaf"
            };
            container.Add(leafButton);
            return container;
        }

        private void OnAddBranchClickedHandler(TreePrefabBuilder.Branch branch, VisualElement root, SerializedProperty property)
        {
            SerializedProperty childrenProperty = property.FindPropertyRelative("children");
            childrenProperty.arraySize++;
            childrenProperty.GetArrayElementAtIndex(childrenProperty.arraySize - 1).managedReferenceValue =
                branch?.AddBranch();
            EditorUtility.SetDirty(branch?.Builder);
            property.serializedObject.ApplyModifiedProperties();
            root.MarkDirtyRepaint();
        }
        
        private void OnAddLeafClickedHandler(TreePrefabBuilder.Branch branch, VisualElement root)
        {
            branch?.AddLeaf();
        }*/
}