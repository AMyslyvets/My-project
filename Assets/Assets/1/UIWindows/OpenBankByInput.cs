using Fiz;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenBankByInput : MonoBehaviour
{
    [SerializeField] private SimpleWindowsManager windowsManager;
    [SerializeField] private string bankWindowId = "Bank";

    private DefaultInputActions input;
    private InputAction bankAction;

    private void Awake()
    {
        input = new DefaultInputActions();
        bankAction = input.DefaultMap.Bank;

        bankAction.performed += OnBankPerformed;
    }

    private void OnEnable()
    {
        input.Enable();
        InputController.OnEscape += CloseBank;
    }

    private void OnDisable()
    {
        if (input != null)
            input.Disable();

        InputController.OnEscape -= CloseBank;
    }

    private void OnDestroy()
    {
        if (bankAction != null)
            bankAction.performed -= OnBankPerformed;
    }

    private void OnBankPerformed(InputAction.CallbackContext context)
    {
        var bankWindow = windowsManager.GetWindowById(bankWindowId);

        if (bankWindow == null)
        {
            Debug.LogWarning($"Window with id '{bankWindowId}' not found.");
            return;
        }

        if (bankWindow.gameObject.activeSelf)
            CloseBank();
        else
            OpenBank();
    }

    private void OpenBank()
    {
        windowsManager.OpenWindow(bankWindowId);

        UIState.IsWindowOpened = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CloseBank()
    {
        var bankWindow = windowsManager.GetWindowById(bankWindowId);

        if (bankWindow == null || !bankWindow.gameObject.activeSelf)
            return;

        windowsManager.CloseWindow(bankWindowId);

        UIState.IsWindowOpened = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}