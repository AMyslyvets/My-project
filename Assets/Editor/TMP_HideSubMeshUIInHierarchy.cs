#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

[InitializeOnLoad]
public static class TMP_SubMeshUI_Hider
{
    private static bool _temporarilyDisabled = false;
    private static double _nextRun;

    static TMP_SubMeshUI_Hider()
    {
        EditorApplication.update += Tick;
        EditorApplication.delayCall += HideAll;
    }

    // Кнопка временного показа (для дебага)
    [MenuItem("Tools/TMP/Show TMP SubMeshUI (Temporary)")]
    private static void ToggleTemporary()
    {
        _temporarilyDisabled = !_temporarilyDisabled;

        if (_temporarilyDisabled)
        {
            ShowAll();
            Debug.Log("TMP SubMeshUI: TEMPORARILY VISIBLE");
        }
        else
        {
            HideAll();
            Debug.Log("TMP SubMeshUI: HIDDEN");
        }
    }

    static void Tick()
    {
        if (_temporarilyDisabled) return;

        // Раз в 0.5 сек достаточно
        if (EditorApplication.timeSinceStartup < _nextRun) return;
        _nextRun = EditorApplication.timeSinceStartup + 0.5;

        HideAll();
    }

    static void HideAll()
    {
        var subMeshes = Resources.FindObjectsOfTypeAll<TMP_SubMeshUI>();
        bool changed = false;

        foreach (var sm in subMeshes)
        {
            if (!sm) continue;

            var go = sm.gameObject;

            if (EditorUtility.IsPersistent(go)) continue;

            var desired = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;

            if ((go.hideFlags & desired) != desired)
            {
                go.hideFlags |= desired;
                changed = true;
            }
        }

        if (changed) Repaint();
    }

    static void ShowAll()
    {
        var subMeshes = Resources.FindObjectsOfTypeAll<TMP_SubMeshUI>();
        bool changed = false;

        foreach (var sm in subMeshes)
        {
            if (!sm) continue;

            var go = sm.gameObject;
            if (EditorUtility.IsPersistent(go)) continue;

            var remove = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;

            if ((go.hideFlags & remove) != 0)
            {
                go.hideFlags &= ~remove;
                changed = true;
            }
        }

        if (changed) Repaint();
    }

    static void Repaint()
    {
        EditorApplication.DirtyHierarchyWindowSorting();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif