using UnityEngine;

public class SimpleWindow : MonoBehaviour
{
    [SerializeField] private string _windowId;

    [Header("Animations")]
    [SerializeField] private Animation _animation;
    [SerializeField] private string _openAnimName;
    [SerializeField] private string _closeAnimName;

    public string WindowId => _windowId;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (_animation != null && !string.IsNullOrEmpty(_openAnimName) && _animation.GetClip(_openAnimName) != null)
        {
            _animation.Play(_openAnimName);
        }
    }

    public void Close()
    {
        if (_animation != null && !string.IsNullOrEmpty(_closeAnimName) && _animation.GetClip(_closeAnimName) != null)
        {
            _animation.Play(_closeAnimName);

            float clipLength = _animation[_closeAnimName].length;
            CancelInvoke(nameof(HideWindow));
            Invoke(nameof(HideWindow), clipLength);
        }
        else
        {
            HideWindow();
        }
    }

    private void HideWindow()
    {
        gameObject.SetActive(false);
    }
}