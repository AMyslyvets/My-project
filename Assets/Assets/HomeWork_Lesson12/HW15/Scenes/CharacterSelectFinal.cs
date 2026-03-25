using UnityEngine;

public class CharacterSelectFinal : MonoBehaviour
{
    [SerializeField] private KeyCode shooterKey = KeyCode.F1;
    [SerializeField] private KeyCode mageKey = KeyCode.F2;

    [Header("Character Roots")]
    [SerializeField] private GameObject shooterRoot;
    [SerializeField] private GameObject mageRoot;

    [Header("Cameras")]
    [SerializeField] private GameObject shooterCamera;
    [SerializeField] private GameObject mageCamera;

    [Header("Mage combat (optional)")]
    [SerializeField] private Behaviour mageCombat; // WaterCombatController

    private void Start()
    {
        SelectShooter();
    }

    private void Update()
    {
        if (Input.GetKeyDown(shooterKey)) SelectShooter();
        if (Input.GetKeyDown(mageKey)) SelectMage();
    }

    private void SelectShooter()
    {
        SetCharacterEnabled(shooterRoot, true);
        SetCharacterEnabled(mageRoot, false);

        SetCameraState(shooterCamera, true);
        SetCameraState(mageCamera, false);

        if (mageCombat) mageCombat.enabled = false;
    }

    private void SelectMage()
    {
        SetCharacterEnabled(shooterRoot, false);
        SetCharacterEnabled(mageRoot, true);

        SetCameraState(shooterCamera, false);
        SetCameraState(mageCamera, true);

        if (mageCombat) mageCombat.enabled = true;
    }

    private void SetCameraState(GameObject cameraObject, bool value)
    {
        if (!cameraObject) return;
        cameraObject.SetActive(value);
    }

    private void SetCharacterEnabled(GameObject root, bool value)
    {
        if (!root) return;

        var cc = root.GetComponentInChildren<CharacterController>(true);
        if (cc) cc.enabled = value;

        var scripts = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var s in scripts)
        {
            if (!s) continue;
            if (s == this) continue;

            if (s.GetType().Name.Contains("Controller") ||
                s.GetType().Name.Contains("Input") ||
                s.GetType().Name.Contains("Move") ||
                s.GetType().Name.Contains("Motor"))
            {
                s.enabled = value;
            }
        }
    }
}