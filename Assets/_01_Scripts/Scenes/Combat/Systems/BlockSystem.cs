using System;
using System.Collections;

/// <summary>
/// Owns the "Block" core mechanic (Slay the Spire style).
/// 
/// - Block is consumed before HP on Damage().
/// - Player Block is cleared at the start of the player's turn.
/// - Enemy Block is cleared at the start of the enemy turn.
///   (So enemies can gain Block during their turn and keep it into the player's turn.)
/// </summary>
public class BlockSystem : Singleton<BlockSystem>
{
    private IDisposable enemyTurnPreSub;
    private IDisposable enemyTurnPostSub;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddBlockGA>(AddBlockPerformer);

        // Turn transition hooks:
        enemyTurnPreSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnPre, ReactionTiming.PRE);
        enemyTurnPostSub = ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnPost, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddBlockGA>();

        enemyTurnPreSub?.Dispose();
        enemyTurnPreSub = null;

        enemyTurnPostSub?.Dispose();
        enemyTurnPostSub = null;
    }

    public void AddBlock(CombatantView target, int amount)
    {
        if (target == null || amount <= 0)
            return;

        // Later you can apply modifiers here (Dexterity/Frail/etc.).
        target.AddBlock(amount);
    }

    private IEnumerator AddBlockPerformer(AddBlockGA ga)
    {
        foreach (var target in ga.Targets)
        {
            AddBlock(target, ga.Amount);
            yield return null;
        }
    }

    // Enemy turn is about to start => clear all enemies' block (start-of-turn rule).
    private void OnEnemyTurnPre(EnemyTurnGA _)
    {
        var enemySystem = EnemySystem.Instance;
        if (enemySystem == null)
            return;

        foreach (var enemy in enemySystem.Enemies)
            enemy?.ClearBlock();
    }

    // Enemy turn finished => player's turn is about to start => clear player block (start-of-turn rule).
    private void OnEnemyTurnPost(EnemyTurnGA _)
    {
        HeroSystem.Instance?.HeroView?.ClearBlock();
    }
}
