using System.Collections.Generic;


/// <summary>
/// Single source of truth for whether a card can be played.
/// UI can query this for glow; gameplay validates again server-side in CardSystem.
/// </summary>
public class CardPlayabilitySystem : Singleton<CardPlayabilitySystem>
{
    public CardPlayabilityResult EvaluateStart(Card card, CombatantView caster)
    {
        var result = new CardPlayabilityResult();

        if (card == null)
        {
            result.AddReason(CardPlayFailCode.NoCard, "No card");
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

        EvaluateCustomConditions(result, new CardPlayabilityContext(card, caster, null, CardPlayPhase.StartPlay));
        return result;
    }

    public CardPlayabilityResult EvaluateCommit(Card card, CombatantView caster, EnemyView manualTarget)
    {
        var result = new CardPlayabilityResult();

        if (card == null)
        {
            result.AddReason(CardPlayFailCode.NoCard, "No card");
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
            if (manualTarget == null)
                result.AddReason(CardPlayFailCode.TargetRequired, "Select a target");
            else if (!IsValidManualTarget(manualTarget))
                result.AddReason(CardPlayFailCode.InvalidTarget, "Invalid target");
        }


        EvaluateCustomConditions(result, new CardPlayabilityContext(card, caster, manualTarget, CardPlayPhase.CommitPlay));
        return result;
    }

    public bool CanStartPlay(Card card, CombatantView caster) => EvaluateStart(card, caster).CanPlay;
    public bool CanCommitPlay(Card card, CombatantView caster, EnemyView manualTarget) => EvaluateCommit(card, caster, manualTarget).CanPlay;

    public bool HasAnyValidManualTargets()
    {
        return HasAnyManualTargetsInternal();
    }

    private bool HasAnyManualTargetsInternal()
    {
        var enemies = EnemySystem.Instance != null ? EnemySystem.Instance.Enemies : null;
        return enemies != null && enemies.Count > 0;
    }

    private bool IsValidManualTarget(EnemyView target)
    {
        var enemies = EnemySystem.Instance != null ? EnemySystem.Instance.Enemies : null;
        return enemies != null && enemies.Contains(target);
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