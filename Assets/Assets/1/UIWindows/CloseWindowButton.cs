using UnityEngine;

public class CloseWindowButton : MonoBehaviour
{
    [SerializeField] private SimpleWindow _window;

    public void CloseWindow()
    {
        if (_window == null)
            return;

        _window.Close();

        UIState.IsWindowOpened = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}