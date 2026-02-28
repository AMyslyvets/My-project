using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fiz
{
    public struct Destructable
    {
        public GameObject gameObject;
        public Transform rendererTransform;
        public MeshRenderer meshRenderer;
    }
    
    public class DestructableSystem : MonoBehaviour
    {
        [SerializeField] private string _emissionName;
        [SerializeField] private float _destructionDuration;
        [SerializeField] private AnimationCurve _destructionScaleCurve;
        [SerializeField] private Gradient _destructionEmissionGradient;
        [SerializeField] private Material[] _materials;
        
        private Dictionary<Rigidbody, Destructable> _destructables = new();
        
        private int _emissionId;
        
        private void Awake()
        {
            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < rigidbodies.Length; ++i)
            {
                Rigidbody rigidbody = rigidbodies[i];
                MeshRenderer meshRenderer = rigidbody.GetComponentInChildren<MeshRenderer>();
                _destructables.Add(rigidbody,
                    new Destructable()
                        { gameObject = rigidbody.gameObject, meshRenderer = meshRenderer, rendererTransform = meshRenderer.transform});
            }
            
            _emissionId = Shader.PropertyToID(_emissionName);

            for (int i = 0; i < _materials.Length; ++i)
            {
                _materials[i].EnableKeyword("_EMISSION");
            }
        }

        public void Destruct(Rigidbody rigidbody)
        {
            if (rigidbody == null)
                return;
            if (!_destructables.TryGetValue(rigidbody, out Destructable destructable))
                return;
            StartCoroutine(DestructRoutine(destructable));
            _destructables.Remove(rigidbody);
        }

        private IEnumerator DestructRoutine(Destructable destructable)
        {
            MaterialPropertyBlock propertyBlock = new();
            //destructable.meshRenderer.SetPropertyBlock(propertyBlock);

            EvaluateDestructable(destructable, propertyBlock, 0f);
            
            float time = 0f;
            float reciprocal = 1f / _destructionDuration;
            while (time < _destructionDuration)
            {
                EvaluateDestructable(destructable, propertyBlock, time * reciprocal);
                yield return null;
                time += Time.deltaTime;
            }
            EvaluateDestructable(destructable, propertyBlock, 1f);
            Destroy(destructable.gameObject);
        }

        private void EvaluateDestructable(Destructable destructable, MaterialPropertyBlock propertyBlock, float progress)
        {
            propertyBlock.SetColor(_emissionId, _destructionEmissionGradient.Evaluate(progress));
            destructable.meshRenderer.SetPropertyBlock(propertyBlock);
            /*for (int i = 0; i < destructable.meshRenderer.sharedMaterials.Length; ++i)
            {
                destructable.meshRenderer.SetPropertyBlock(propertyBlock, i);
            }*/
            destructable.rendererTransform.localScale = Vector3.one * _destructionScaleCurve.Evaluate(progress);
        }
    }
}