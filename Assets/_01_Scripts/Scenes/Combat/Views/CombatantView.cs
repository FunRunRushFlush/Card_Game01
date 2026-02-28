using DG.Tweening;
using Game.Logging;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private StatusEffectsManagerUI statusEffectsManagerUI;

    [Header("Block")]
    [SerializeField] private GameObject blockRoot;
    [SerializeField] private TMP_Text blockText;


    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }

    public int CurrentBlock { get; private set; }

    private Dictionary<StatusEffectType, int> statusEffects = new();

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        CurrentBlock = 0;

        spriteRenderer.sprite = image;
        healthBar.SetMaxHealth(MaxHealth);
        UpdateHealthbarUI();
    }


    private void UpdateHealthbarUI()
    {
        healthBar.SetHealthAndBlock(CurrentHealth, CurrentBlock);
    }
    /// <summary>
    /// Regular damage that is absorbed by Block first, then reduces HP.
    /// If you want damage to bypass Block (e.g. Poison/HP loss), use LoseHealth().
    /// </summary>
    public void Damage(int damageAmount)
    {
        if (!this)
        {
            Log.Warn(LogCat.General, () => "Object does not Exist anymore");
            return;
        }
        if (damageAmount <= 0)
            return;

        int remainingDamage = damageAmount;

        // 1) Absorb with Block
        if (CurrentBlock > 0)
        {
            int absorbed = Mathf.Min(CurrentBlock, remainingDamage);
            CurrentBlock -= absorbed;
            remainingDamage -= absorbed;
            UpdateHealthbarUI();
        }

        // 2) Apply remaining to HP
        if (remainingDamage > 0)
        {
            CurrentHealth -= remainingDamage;
            if (CurrentHealth < 0)
                CurrentHealth = 0;

            UpdateHealthbarUI();
        }

        if(transform != null)
        {
            transform.DOShakePosition(0.2f, 0.5f);
        }
    }
    /// <summary>
    /// HP loss that bypasses Block.
    /// Useful for mechanics like Poison or self-damage that should ignore Block.
    /// </summary>
    public void LoseHealth(int amount)
    {
        if (amount <= 0)
            return;

        CurrentHealth -= amount;
        if (CurrentHealth < 0)
            CurrentHealth = 0;

        transform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthbarUI();
    }

    public void AddBlock(int amount)
    {
        if (amount <= 0)
            return;

        CurrentBlock += amount;
        if (CurrentBlock < 0)
            CurrentBlock = 0;

        UpdateHealthbarUI();
    }

    public void ClearBlock()
    {
        if (CurrentBlock == 0)
            return;

        CurrentBlock = 0;
        UpdateHealthbarUI();
    }



    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        if (!this) return;                
        if (stackCount <= 0) return;

  
        if (CurrentHealth <= 0) return;

        if (!statusEffects.ContainsKey(type))
            statusEffects[type] = 0;

        statusEffects[type] += stackCount;

        if (statusEffectsManagerUI)
        {
            statusEffectsManagerUI.UpdateStatusEfectUI(type, statusEffects[type]);
        }
    }

    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        if (!this) return;
        if (CurrentHealth <= 0) return;

        if (!statusEffects.ContainsKey(type)) return;

        statusEffects[type] -= stackCount;
        if (statusEffects[type] <= 0)
            statusEffects.Remove(type);

        if (statusEffectsManagerUI)
        {
            statusEffectsManagerUI.UpdateStatusEfectUI(type, GetStatusEffectStacks(type));
        }
    }

    public int GetStatusEffectStacks(StatusEffectType type)
    {
        if (statusEffects.ContainsKey(type))
            return statusEffects[type];

        return 0;
    }

}
