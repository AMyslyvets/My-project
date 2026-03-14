using UnityEngine;

public class TestEnum : MonoBehaviour
{
    public enum EnemyType
    {
        Zombie,
        Skeleton,
        Demon
    }

    public EnemyType enemy;

    void Start()
    {
        if (enemy == EnemyType.Zombie)
        {
            Debug.Log("Spawn Zombie");
        }
    }
}