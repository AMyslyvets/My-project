using UnityEngine;
using UnityEngine.SceneManagement;



    public class LoadSceneButton : MonoBehaviour
    {
        [SerializeField] private string _sceneName;

        public void LoadScene()
        {
            if (!string.IsNullOrEmpty(_sceneName))
            {
                SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogWarning("Scene name is not assigned!");
            }
        }
    }
