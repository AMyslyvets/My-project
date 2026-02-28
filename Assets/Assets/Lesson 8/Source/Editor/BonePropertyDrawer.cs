using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lesson8.Editor
{
    /*[CustomPropertyDrawer(typeof(TreePrefabBuilder.Bone))]
    public class BonePropertyDrawer : PropertyDrawer
    {
        /*public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            TreePrefabBuilder.Bone bone = property.managedReferenceValue as TreePrefabBuilder.Bone;
            
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
                Button removeButton = new Button(() => RemoveSelf(bone, box))
                {
                    text = "Remove Node"
                };
                box.Add(removeButton);
            }

            box.Add(new Label("Children"));
            
            SerializedProperty childrenProperty = property.FindPropertyRelative("children");
            
            for (int i = 0; i < childrenProperty.arraySize; ++i)
            {
                box.Add(new PropertyField(childrenProperty.GetArrayElementAtIndex(i)));
            }
            
            return box;
        }#1#
        
        private void RemoveSelf(TreePrefabBuilder.Bone bone, VisualElement root)
        {
            bone.RemoveSelf();
            root.MarkDirtyRepaint();
        }
    }*/
}