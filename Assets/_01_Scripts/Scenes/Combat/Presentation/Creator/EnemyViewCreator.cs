using UnityEngine;

public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
    [SerializeField] private EnemyView defaultEnemyViewPrefab;

    public EnemyView CreateEnemyView(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        var prefab = enemyData.ViewPrefab != null ? enemyData.ViewPrefab : defaultEnemyViewPrefab;

        EnemyView enemyView = Instantiate(prefab, position, rotation);
        enemyView.Setup(enemyData);
        return enemyView;
    }
}
