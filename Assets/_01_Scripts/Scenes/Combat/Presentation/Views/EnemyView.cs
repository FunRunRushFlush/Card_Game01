using Game.Logging;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [Header("Intent UI")]
    [SerializeField] private SpriteRenderer intentIconImage;
    [SerializeField] private TMP_Text intentValueText;

    [Header("Animation")]
    [SerializeField] private MonoBehaviour animatorBehaviour; // must implement ICombatantAnimator
    public ICombatantAnimator Anim { get; private set; }

    private void Awake()
    {
        Anim = animatorBehaviour as ICombatantAnimator;

        if (animatorBehaviour != null && Anim == null)
            Log.Error(LogArea.Combat, () => $"[{name}] animatorBehaviour does not implement ICombatantAnimator", this);
    }

    // Presentation-only init (no AI/stats here)
    public void SetupPresentation(EnemyData enemyData)
    {
        SetupBase(enemyData.Health, enemyData.Image);
        Anim?.SetIdle();

        RenderIntent(default);
    }

    public void RenderIntent(IntentData intent)
    {
        if (intentIconImage != null)
        {
            intentIconImage.sprite = intent.Icon;
            intentIconImage.enabled = intent.Icon != null;
        }

        if (intentValueText != null)
        {
            if (intent.ShowValue)
            {
                intentValueText.gameObject.SetActive(true);
                intentValueText.text = !string.IsNullOrEmpty(intent.ValueText)
                    ? intent.ValueText
                    : intent.Value.ToString();
            }
            else
            {
                intentValueText.gameObject.SetActive(false);
                intentValueText.text = string.Empty;
            }
        }
    }
}