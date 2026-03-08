using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboPointSystem : Singleton<ComboPointSystem>
{
    private const int DefaultMaxComboPoints = 5;

    private int currentComboPoints;
    private int maxComboPoints = DefaultMaxComboPoints;

    public int CurrentComboPoints => currentComboPoints;
    public int MaxComboPoints => maxComboPoints;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<GainComboPointsGA>(GainComboPointsPerformer);
        ActionSystem.AttachPerformer<SpendComboPointsGA>(SpendComboPointsPerformer);
        ActionSystem.AttachPerformer<ResetComboPointsGA>(ResetComboPointsPerformer);
        ActionSystem.AttachPerformer<FinisherDamageGA>(FinisherDamagePerformer);

        CombatContextHelpers.SubscribeInitialized(OnCombatInitialized);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<GainComboPointsGA>();
        ActionSystem.DetachPerformer<SpendComboPointsGA>();
        ActionSystem.DetachPerformer<ResetComboPointsGA>();
        ActionSystem.DetachPerformer<FinisherDamageGA>();

        CombatContextHelpers.UnsubscribeInitialized(OnCombatInitialized);
    }

    private void OnCombatInitialized()
    {
        currentComboPoints = 0;
        maxComboPoints = DefaultMaxComboPoints;
        PublishChanged();
    }

    public bool HasEnough(int amount)
    {
        return currentComboPoints >= amount;
    }

    private IEnumerator GainComboPointsPerformer(GainComboPointsGA ga)
    {
        if (ga.Amount <= 0)
            yield break;

        int oldValue = currentComboPoints;
        currentComboPoints = Mathf.Clamp(currentComboPoints + ga.Amount, 0, maxComboPoints);

        if (currentComboPoints != oldValue)
            PublishChanged();

        yield return null;
    }

    private IEnumerator SpendComboPointsPerformer(SpendComboPointsGA ga)
    {
        if (ga.Amount <= 0)
            yield break;

        int oldValue = currentComboPoints;
        currentComboPoints = Mathf.Clamp(currentComboPoints - ga.Amount, 0, maxComboPoints);

        if (currentComboPoints != oldValue)
            PublishChanged();

        yield return null;
    }

    private IEnumerator ResetComboPointsPerformer(ResetComboPointsGA ga)
    {
        if (currentComboPoints == 0)
            yield break;

        currentComboPoints = 0;
        PublishChanged();

        yield return null;
    }

    private IEnumerator FinisherDamagePerformer(FinisherDamageGA ga)
    {
        int comboPoints = CurrentComboPoints;
        int damage = ga.BaseDamage + comboPoints * ga.DamagePerComboPoint;

        foreach (CombatantId target in ga.Targets)
        {
            ActionSystem.Instance.AddReaction(
                new DealDamageGA(damage, new List<CombatantId> { target }, ga.Caster)
            );
        }

        if (ga.ConsumeAllComboPoints && comboPoints > 0)
        {
            ActionSystem.Instance.AddReaction(new ResetComboPointsGA());
        }

        yield return null;
    }

    private void PublishChanged()
    {
        CombatDomainEventBus.Publish(
            new ComboPointsChangedEvent(currentComboPoints, maxComboPoints)
        );
    }
}