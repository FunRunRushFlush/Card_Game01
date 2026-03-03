using Game.Logging;
using UnityEngine;

public class EndTurnButtonUI : MonoBehaviour
{

    public void OnClick()
    {
        if (CombatPauseGateSystem.Instance != null && CombatPauseGateSystem.Instance.IsPaused)
        {
            Log.Info(LogArea.Combat, () => $"Game is Paused!", this);
            return;
        }
        if (EnemySystem.Instance != null && EnemySystem.Instance.AreAllEnemiesDefeated())
        {
            Log.Info(LogArea.Combat, () => "End Turn ignored: combat already won.", this);
            return;
        }
        Log.Info(LogArea.Combat, () => "End Turn Button clicked.", this);
        EnemyTurnGA enemyTurnGA = new();
        ActionSystem.Instance.Perform(enemyTurnGA);
    }
}
