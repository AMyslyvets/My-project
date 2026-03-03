using UnityEngine;
using Fiz;

public class CharacterSelectHard : MonoBehaviour
{
    [SerializeField] private KeyCode shooterKey = KeyCode.F1;
    [SerializeField] private KeyCode mageKey = KeyCode.F2;

    [Header("Pick the ONLY active mover")]
    [SerializeField] private PlayerController shooterMove;
    [SerializeField] private PlayerController mageMove;

    [Header("Mage extra (optional)")]
    [SerializeField] private MonoBehaviour mageCombat; // WaterCombatController

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
        EnableOnly(shooterMove);
        if (mageCombat) mageCombat.enabled = false;
        Debug.Log("[Select] Shooter");
    }

    private void SelectMage()
    {
        EnableOnly(mageMove);
        if (mageCombat) mageCombat.enabled = true;
        Debug.Log("[Select] Mage");
    }

    private void EnableOnly(PlayerController allowed)
    {
        var all = FindObjectsOfType<PlayerController>(true); // включая выключенные
        foreach (var pc in all)
            pc.enabled = (pc == allowed);
    }
}