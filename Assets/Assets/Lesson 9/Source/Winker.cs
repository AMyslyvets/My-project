using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Lesson9
{
    public class Winker : MonoBehaviour
    {
        [Serializable]
        private struct BlendShapeTarget
        {
            public string blendShapeName;
            public float targetWeight;

            public int BlendShapeIndex { get; set; }
        }

        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

        [SerializeField] private float _duration;
        [SerializeField] private float _delay;
        [SerializeField] private BlendShapeTarget[] _blendShapeTargets;

        [ContextMenu("Print Blend shapes")]
        private void PrintBlendShapes()
        {
            Mesh mesh = _skinnedMeshRenderer.sharedMesh;
            int blendShapeCount = mesh.blendShapeCount;
            StringBuilder outputBuilder = new StringBuilder($"Blend shapes of '{mesh.name}':",
                19 + 50 + (5 + 50 + 3 + 2) * blendShapeCount);
            for (int i = 0; i < blendShapeCount; ++i)
            {
                outputBuilder.Append($"{Environment.NewLine}[{i}] {mesh.GetBlendShapeName(i)}");
            }

            Debug.Log(outputBuilder);
        }

        private void Awake()
        {
            for (int i = 0; i < _blendShapeTargets.Length; ++i)
            {
                _blendShapeTargets[i].BlendShapeIndex =
                    _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(_blendShapeTargets[i].blendShapeName);
            }
        }

        public void Wink(Selectable button)
        {
            StartCoroutine(WinkCycle(button));
        }

        private IEnumerator WinkCycle(Selectable button)
        {
            button.interactable = false;
            yield return WinkRoutine(true);
            yield return new WaitForSeconds(_delay);
            yield return WinkRoutine(false);
            button.interactable = true;
        }

        private IEnumerator WinkRoutine(bool ascend)
        {
            float time = 0f;
            float reciprocalTime = 1f / _duration;

            while (time < _duration)
            {
                EvaluateBlendShapes(time * reciprocalTime, ascend);
                yield return null;
                time += Time.deltaTime;
            }

            EvaluateBlendShapes(1f, ascend);
        }

        private void EvaluateBlendShapes(float progress, bool ascend)
        {
            for (int i = 0; i < _blendShapeTargets.Length; ++i)
                EvaluateBlendShape(_blendShapeTargets[i], progress, ascend);
        }

        private void EvaluateBlendShape(BlendShapeTarget target, float progress, bool ascend)
        {
            float weight = Mathf.Lerp(0f, target.targetWeight, ascend ? progress : 1f - progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(target.BlendShapeIndex, weight);
        }
    }
}