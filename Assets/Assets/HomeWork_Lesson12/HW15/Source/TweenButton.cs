using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{ 
    public class TweenButton : Button
    {
        public float downScale = 0.9f;
        public float duration = 0.5f;
        public Ease ease = Ease.OutElastic;

        protected Tweener m_DownTween;
        protected Tweener m_UpTween;
        

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (m_DownTween == null)
            {
                //m_DownTween = DOTween.To(() => 1f, (x) => this.transform.sc = x, arg.New, duration).SetEase(ease);
                m_DownTween = this.transform.DOScale(new Vector3(downScale, downScale, downScale), duration).SetEase(ease).SetAutoKill(false);
            }
            else
            {
                m_DownTween.Restart();
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (m_UpTween == null)
            {
                //m_DownTween = DOTween.To(() => 1f, (x) => this.transform.sc = x, arg.New, duration).SetEase(ease);
                m_UpTween = this.transform.DOScale(new Vector3(1f, 1f, 1f), duration).SetEase(ease).SetAutoKill(false);
            }
            else
            {
                m_UpTween.Restart();
            }
        }
    }
}