using Game.Logging;
using Game.Scenes.Core;
using System.Collections;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    [SerializeField] private GameObject damageVFX;


    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);

    }


    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        int baseAmount = dealDamageGA.Amount;

        int str = dealDamageGA.Caster != null
            ? dealDamageGA.Caster.GetStatusEffectStacks(StatusEffectType.STRENGTH)
            : 0;

        int weak = dealDamageGA.Caster != null
            ? dealDamageGA.Caster.GetStatusEffectStacks(StatusEffectType.WEAKNESS)
            : 0;

        int modifiedAmount = Mathf.Max(0, baseAmount + str - weak);

        foreach (var target in dealDamageGA.Targets)
        {
            if(!target)
            {
                Log.Warn(LogCat.General, () => "target Object does not Exist anymore");
                continue;
            }
            target.Damage(modifiedAmount);

            if (damageVFX && target)
                Instantiate(damageVFX, target.transform.position, Quaternion.identity);


            yield return new WaitForSeconds(0.15f);

            if (!target)
            {
                Log.Warn(LogCat.General, () => "target Object does not Exist anymore");
                continue;
            }

            if (target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new(enemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else if (target is HeroView heroView)
                {
                    GameFlowController.Current.CombatLost();
                }
                else
                {
                    throw new System.Exception("Should this even happen? (TODO)");
                }
            }
        }
    }

}