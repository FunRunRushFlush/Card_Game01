using UnityEngine;

public class EndTurnButtonUI : MonoBehaviour
{

    public void OnClick()
    {
        if (CombatPauseGateSystem.Instance != null && CombatPauseGateSystem.Instance.IsPaused)
        {
            Debug.Log($"Game is Paused!");
            return;
        }
        if (EnemySystem.Instance != null && EnemySystem.Instance.AreAllEnemiesDefeated())
        {
            Debug.Log("End Turn ignored: combat already won.");
            return;
        }
        Debug.Log("End Turn Button clicked.");
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
