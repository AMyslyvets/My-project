using UnityEngine;

public class CharacterSelectFinal : MonoBehaviour
{
    [SerializeField] private KeyCode shooterKey = KeyCode.F1;
    [SerializeField] private KeyCode mageKey = KeyCode.F2;

    [Header("Character Roots")]
    [SerializeField] private GameObject shooterRoot;
    [SerializeField] private GameObject mageRoot;

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

        if (mageCombat) mageCombat.enabled = false;
    }

    private void SelectMage()
    {
        SetCharacterEnabled(shooterRoot, false);
        SetCharacterEnabled(mageRoot, true);

        if (mageCombat) mageCombat.enabled = true;
    }

    private void SetCharacterEnabled(GameObject root, bool value)
    {
        if (!root) return;

        // 1) Движение: CharacterController
        var cc = root.GetComponentInChildren<CharacterController>(true);
        if (cc) cc.enabled = value;

        // 2) На всякий случай: выключаем ВСЕ MonoBehaviour кроме Renderer/Animator не трогаем
        // (движение может быть в другом скрипте)
        var scripts = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var s in scripts)
        {
            if (!s) continue;

            // не трогаем этот селектор, если вдруг он внутри
            if (s == this) continue;

            // ВАЖНО: не отключаем UI/камера и т.п. (обычно не внутри root персонажа)
            // Отключаем только скрипты, которые могут слушать input
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