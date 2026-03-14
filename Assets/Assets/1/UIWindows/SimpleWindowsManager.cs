using UnityEngine;

public class SimpleWindowsManager : MonoBehaviour
{
    [SerializeField] private SimpleWindow[] _windows;

    public void OpenWindow(string windowId)
    {
        SimpleWindow targetWindow = GetWindowById(windowId);

        if (targetWindow == null)
        {
            Debug.LogWarning($"Window with id '{windowId}' not found.");
            return;
        }

        targetWindow.Open();
    }

    public void CloseWindow(string windowId)
    {
        SimpleWindow targetWindow = GetWindowById(windowId);

        if (targetWindow == null)
        {
            Debug.LogWarning($"Window with id '{windowId}' not found.");
            return;
        }

        targetWindow.Close();
    }

    public SimpleWindow GetWindowById(string windowId)
    {
        for (int i = 0; i < _windows.Length; i++)
        {
            if (_windows[i] == null)
                continue;

            if (_windows[i].WindowId == windowId)
                return _windows[i];
        }

        return null;
    }
}