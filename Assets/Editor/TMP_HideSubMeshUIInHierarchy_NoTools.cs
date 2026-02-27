/*   #if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

[InitializeOnLoad]
public static class TMP_SubMeshUI_Hider
{
    static double _nextRun;

    static TMP_SubMeshUI_Hider()
    {
        EditorApplication.update += Tick;
    }

    static void Tick()
    {
        // Раз в 0.5 сек достаточно (TMP не создаёт их каждую миллисекунду)
        if (EditorApplication.timeSinceStartup < _nextRun) return;
        _nextRun = EditorApplication.timeSinceStartup + 0.5;

        var subMeshes = Resources.FindObjectsOfTypeAll<TMP_SubMeshUI>();
        bool changed = false;

        foreach (var sm in subMeshes)
        {
            if (!sm) continue;

            var go = sm.gameObject;

            // Не трогаем ассеты на диске (prefab asset/subasset)
            if (EditorUtility.IsPersistent(go)) continue;

            // HideInHierarchy + не сохранять в сцену как “важные данные”
            var desired = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;

            if ((go.hideFlags & desired) != desired)
            {
                go.hideFlags |= desired;
                changed = true;
            }
        }

        if (changed)
        {
            EditorApplication.DirtyHierarchyWindowSorting();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
#endif  */