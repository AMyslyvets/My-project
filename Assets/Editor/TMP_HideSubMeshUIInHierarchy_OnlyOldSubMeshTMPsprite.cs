/*    #if UNITY_EDITOR
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
        EditorApplication.delayCall += HideAllSpriteSubMeshes;
    }

    // Временный показ спрайтовых сабмешей (для дебага)
    [MenuItem("Tools/TMP/Show TMP Sprite SubMeshes (Temporary)")]
    private static void ToggleTemporary()
    {
        _temporarilyDisabled = !_temporarilyDisabled;

        if (_temporarilyDisabled)
        {
            ShowAllSpriteSubMeshes();
            Debug.Log("TMP Sprite SubMeshes: TEMPORARILY VISIBLE");
        }
        else
        {
            HideAllSpriteSubMeshes();
            Debug.Log("TMP Sprite SubMeshes: HIDDEN");
        }
    }

    private static void Tick()
    {
        if (_temporarilyDisabled) return;

        // раз в 0.5 сек достаточно
        if (EditorApplication.timeSinceStartup < _nextRun) return;
        _nextRun = EditorApplication.timeSinceStartup + 0.5;

        HideAllSpriteSubMeshes();
    }

    private static bool IsSpriteSubMesh(TMP_SubMeshUI sm)
    {
        if (!sm) return false;

        // Основной признак: материал/шейдер "TextMeshPro/Sprite"
        var mat = sm.sharedMaterial != null ? sm.sharedMaterial : sm.materialForRendering;
        if (mat != null && mat.shader != null)
        {
            var shaderName = mat.shader.name;
            if (!string.IsNullOrEmpty(shaderName) && shaderName.Contains("TextMeshPro/Sprite"))
                return true;
        }

        // Запасной вариант: имя объекта обычно содержит [TextMeshPro/Sprite]
        // (на случай, если материал ещё не назначен в момент проверки)
        var n = sm.gameObject.name;
        if (!string.IsNullOrEmpty(n) && n.Contains("TextMeshPro/Sprite"))
            return true;

        return false;
    }

    private static void HideAllSpriteSubMeshes()
    {
        var subMeshes = Resources.FindObjectsOfTypeAll<TMP_SubMeshUI>();
        bool changed = false;

        foreach (var sm in subMeshes)
        {
            if (!sm) continue;
            if (!IsSpriteSubMesh(sm)) continue;

            var go = sm.gameObject;

            // Не трогаем ассеты на диске
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

    private static void ShowAllSpriteSubMeshes()
    {
        var subMeshes = Resources.FindObjectsOfTypeAll<TMP_SubMeshUI>();
        bool changed = false;

        foreach (var sm in subMeshes)
        {
            if (!sm) continue;
            if (!IsSpriteSubMesh(sm)) continue;

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

    private static void Repaint()
    {
        EditorApplication.DirtyHierarchyWindowSorting();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif   */