using Game.Logging;
using Game.Scenes.Core;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private GameObject chest;

    private void Start()
    {
        EnemySystem.Instance.AllEnemiesDefeated += OnAllEnemiesDefeated;
    }
    private void OnDisable()
    {
        if (EnemySystem.Instance != null)
            EnemySystem.Instance.AllEnemiesDefeated -= OnAllEnemiesDefeated;
    }

    private void OnAllEnemiesDefeated()
    {
        Log.Debug(LogCat.General, () => "AllEnemiesDefeated!");
        if (chest != null) 
            chest.SetActive(true);
    }


    public void CombatWon()
    {
        GameFlowController.Current.CombatWon();

    }
    public void CombatLost()
    {
        GameFlowController.Current.CombatLost();
    }
}