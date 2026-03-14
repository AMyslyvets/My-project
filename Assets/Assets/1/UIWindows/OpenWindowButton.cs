using UnityEngine;

public class OpenWindowButton : MonoBehaviour
{
    [SerializeField] private SimpleWindowsManager _windowsManager;
    [SerializeField] private string _windowId;

    public void OpenWindow()
    {
        if (_windowsManager == null)
        {
            Debug.LogWarning("WindowsManager is not assigned.");
            return;
        }

        _windowsManager.OpenWindow(_windowId);
    }
}