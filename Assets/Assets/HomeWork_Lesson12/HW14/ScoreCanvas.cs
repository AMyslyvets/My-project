using System;
using TMPro;
using UnityEngine;

namespace Fiz
{
    public class ScoreCanvas : MonoBehaviour
    {
        [SerializeField] private ScoreSystem _scoreSystem;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [SerializeField] private TextMeshProUGUI _kills;
        [SerializeField] private TextMeshProUGUI _deaths;
        [SerializeField] private TextMeshProUGUI _assists;

        [SerializeField] private TextMeshProUGUI _accuracy;

        private bool _updateRequested;
        
        private void Start()
        {
            _scoreSystem.OnDataUdpated += DataUpdateHandler;

            Fiz.InputController.OnScoreInput += ScoreInputHandler;
            
            _updateRequested = true;
        }

        private void DataUpdateHandler()
        {
            _updateRequested = true;
        }

        private void LateUpdate()
        {
            if (!_updateRequested)
                return;
            _updateRequested = false;

            _kills.text = _scoreSystem.KDA.x.ToString();
            _deaths.text = _scoreSystem.KDA.y.ToString();
            _assists.text = _scoreSystem.KDA.z.ToString();
            _accuracy.text = _scoreSystem.Accuracy.ToString();
        }

        private void ScoreInputHandler(bool show)
        {
            _canvasGroup.alpha = show ? 1f : 0f;
        }
    }
}