using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Lesson9
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private bool _useCrossFade;
        [SerializeField] private float _crossFadeTime;
        [SerializeField] private string[] _animationNames;

        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private Vector2 _scrollPosition;
        
        [ContextMenu("Fill Names")]
        private void FillNames()
        {
#if UNITY_EDITOR
            AnimatorController controller = _animator.runtimeAnimatorController as AnimatorController;
            if (controller == null)
            {
                Debug.LogError("Please select an Animator Controller in the Project window.");
                return;
            }

            ChildAnimatorState[] states = controller.layers[0].stateMachine.states;
            _animationNames = new string[states.Length];
            for (int i = 0; i < states.Length; ++i)
            {
                _animationNames[i] = states[i].state.name;
            }
#endif
        }

        private void OnGUI()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.padding = new RectOffset(12, 12, 0, 0);
            }
            
            GUILayout.BeginArea(new Rect(16, 16, 200, 200));
            GUI.Box(new Rect(0, 0, 200, 200), string.Empty);
            GUILayout.Label("Movers controller", _labelStyle);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            for (int i = 0; i < _animationNames.Length; ++i)
            {
                string animation = _animationNames[i];
                if (GUILayout.Button(animation, _buttonStyle))
                {
                    if (_useCrossFade)
                        _animator.CrossFadeInFixedTime(animation, _crossFadeTime);
                    else
                        _animator.Play(animation);
                }
            }
            GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}