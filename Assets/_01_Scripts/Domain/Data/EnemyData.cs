using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int Health { get; private set; }

    [Header("Combat Values")]
    [field: SerializeField] public int AttackValue { get; private set; } = 6;
    [field: SerializeField] public int MultiAttackValue { get; private set; } = 3;

    [field: SerializeField] public int BlockValue { get; private set; } = 6;
    [field: SerializeField] public int BurnValue { get; private set; } = 2;
    [field: SerializeField] public int PoisonValue { get; private set; } = 2;

    [field: SerializeField] public int StrengthValue { get; private set; } = 1;
    [field: SerializeField] public int WeakValue { get; private set; } = 1;


    [Header("View")]
    [field: SerializeField] public EnemyView ViewPrefab { get; private set; }

    [Header("AI")]
    [field: SerializeField] public EnemyBehaviourSO Behaviour { get; private set; }

}
