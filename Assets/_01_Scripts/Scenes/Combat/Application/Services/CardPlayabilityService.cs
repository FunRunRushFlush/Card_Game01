/// <summary>
/// Single source of truth for whether a card can be played.
/// UI can query this for glow; gameplay validates again centrally in CardSystem.
/// </summary>
public class CardPlayabilityService : Singleton<CardPlayabilityService>
{
    public CardPlayabilityResult EvaluateStart(Card card, CombatantId casterId)
    {
        var result = new CardPlayabilityResult();

        if (card == null)
        {
            result.AddReason(CardPlayFailCode.NoCard, "No card");
            return result;
        }

        if (CombatPauseGateSystem.Instance != null && CombatPauseGateSystem.Instance.IsPaused)
        {
            result.AddReason(CardPlayFailCode.SystemNotReady, "Game is paused");
            return result;
        }

        if (EnemySystem.Instance != null && EnemySystem.Instance.AreAllEnemiesDefeated())
        {
            result.AddReason(CardPlayFailCode.CombatEnded, "Combat is already won");
            return result;
        }

        if (ManaSystem.Instance == null)
        {
            result.AddReason(CardPlayFailCode.SystemNotReady, "Mana system not ready");
            return result;
        }

        if (!ManaSystem.Instance.HasEnoughMana(card.Mana))
            result.AddReason(CardPlayFailCode.NotEnoughMana, "Not enough mana");

        if (card.HasManualTargetEffects)
        {
            if (!HasAnyManualTargetsInternal())
                result.AddReason(CardPlayFailCode.NoValidTargets, "No valid targets");
        }

        EvaluateCustomConditions(
            result,
            new CardPlayabilityContext(card, casterId, null, CardPlayPhase.StartPlay)
        );

        return result;
    }

    public CardPlayabilityResult EvaluateCommit(Card card, CombatantId casterId, CombatantId? manualTargetId)
    {
        var result = new CardPlayabilityResult();

        if (card == null)
        {
            result.AddReason(CardPlayFailCode.NoCard, "No card");
            return result;
        }

        if (CombatPauseGateSystem.Instance != null && CombatPauseGateSystem.Instance.IsPaused)
        {
            result.AddReason(CardPlayFailCode.SystemNotReady, "Game is paused");
            return result;
        }

        if (EnemySystem.Instance != null && EnemySystem.Instance.AreAllEnemiesDefeated())
        {
            result.AddReason(CardPlayFailCode.CombatEnded, "Combat is already won");
            return result;
        }

        if (ManaSystem.Instance == null)
        {
            result.AddReason(CardPlayFailCode.SystemNotReady, "Mana system not ready");
            return result;
        }

        if (!ManaSystem.Instance.HasEnoughMana(card.Mana))
            result.AddReason(CardPlayFailCode.NotEnoughMana, "Not enough mana");

        if (card.HasManualTargetEffects)
        {
            if (!manualTargetId.HasValue)
                result.AddReason(CardPlayFailCode.TargetRequired, "Select a target");
            else if (!IsValidManualTarget(manualTargetId.Value))
                result.AddReason(CardPlayFailCode.InvalidTarget, "Invalid target");
        }

        EvaluateCustomConditions(
            result,
            new CardPlayabilityContext(card, casterId, manualTargetId, CardPlayPhase.CommitPlay)
        );

        return result;
    }

    public bool CanStartPlay(Card card, CombatantId casterId)
        => EvaluateStart(card, casterId).CanPlay;

    public bool CanCommitPlay(Card card, CombatantId casterId, CombatantId? manualTargetId)
        => EvaluateCommit(card, casterId, manualTargetId).CanPlay;

    public bool HasAnyValidManualTargets()
    {
        return HasAnyManualTargetsInternal();
    }

    private bool HasAnyManualTargetsInternal()
    {
        var enemySystem = EnemySystem.Instance;
        if (enemySystem == null)
            return false;

        if (enemySystem.AreAllEnemiesDefeated())
            return false;

        var state = CombatContextService.Instance != null ? CombatContextService.Instance.State : null;
        if (state == null)
            return false;

        var enemyIds = enemySystem.EnemyIds;
        if (enemyIds == null || enemyIds.Count == 0)
            return false;

        foreach (var enemyId in enemyIds)
        {
            if (state.TryGet(enemyId, out var enemyState) && enemyState != null && enemyState.Health > 0)
                return true;
        }

        return false;
    }

    private bool IsValidManualTarget(CombatantId targetId)
    {
        var enemySystem = EnemySystem.Instance;
        if (enemySystem == null)
            return false;

        var enemyIds = enemySystem.EnemyIds;
        if (enemyIds == null || enemyIds.Count == 0)
            return false;

        bool isEnemy = false;
        for (int i = 0; i < enemyIds.Count; i++)
        {
            if (enemyIds[i].Value == targetId.Value)
            {
                isEnemy = true;
                break;
            }
        }

        if (!isEnemy)
            return false;

        var state = CombatContextService.Instance != null ? CombatContextService.Instance.State : null;
        if (state == null)
            return false;

        if (!state.TryGet(targetId, out var targetState) || targetState == null)
            return false;

        return targetState.Health > 0;
    }

    private void EvaluateCustomConditions(CardPlayabilityResult result, in CardPlayabilityContext context)
    {
        var conditions = context.Card.PlayConditions;
        if (conditions == null || conditions.Count == 0)
            return;

        foreach (var condition in conditions)
        {
            if (condition == null)
                continue;

            if (!condition.IsMet(context))
            {
                var reason = condition.GetFailReason(context);
                result.Reasons.Add(reason);
            }
        }
    }
}