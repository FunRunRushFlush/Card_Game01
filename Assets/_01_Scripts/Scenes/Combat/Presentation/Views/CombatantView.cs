using DG.Tweening;
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

    public CombatantId Id { get; private set; }
    public void AssignId(CombatantId id) => Id = id;

    // Shared presentation setup used by HeroView and EnemyView.
    // Keeps CombatantView presentation-only (no gameplay state stored here).
    protected void SetupBase(int maxHealth, Sprite image)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = image;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }


    public void Render(CombatantState state)
    {
        healthBar.SetHealthAndBlock(state.Health, state.Block);

        // Optional: Status-UI komplett neu zeichnen
        // (oder nur Delta-Updates, wenn ihr später Events einführt)
        // statusEffectsManagerUI.RenderAll(state);  // falls ihr sowas baut
    }

    public void PlayHitFeedback()
    {
        if (transform != null)
            transform.DOShakePosition(0.2f, 0.5f);
    }
}