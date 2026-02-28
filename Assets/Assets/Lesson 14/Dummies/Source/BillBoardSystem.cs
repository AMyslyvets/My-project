#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dummies
{
    public class BillBoardSystem : MonoBehaviour
    {
        [SerializeField] protected CameraSystem _cameraSystem;

        [SerializeField] protected BillboardBase[] _horizontalBillboards;
        
        [ContextMenu("Find Billboards")]
        private void FindBillboards()
        {
            _horizontalBillboards = FindObjectsOfType<BillboardBase>(true);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        protected virtual void Awake()
        {
            for (int i = 0; i < _horizontalBillboards.Length; ++i)
                _horizontalBillboards[i].SetCamera(_cameraSystem.Camera);
        }
    }
}