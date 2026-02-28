using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;

    [Header("Intent UI")]
    [SerializeField] private SpriteRenderer intentIconImage;
    [SerializeField] private TMP_Text intentValueText;

    [Header("Animation")]
    [SerializeField] private MonoBehaviour animatorBehaviour; // must implement ICombatantAnimator
    public ICombatantAnimator Anim { get; private set; }

    public int AttackValue { get; set; }
    public int MultiAttackValue { get; set; }

    public int BlockValue { get; private set; }
    public int BurnValue { get; private set; } = 2;
    public int PoisonValue { get; private set; } = 2;

    public int StrengthValue { get; private set; } = 1;
    public int WeakValue { get; private set; } = 1;

    public EnemyBehaviourSO Behaviour { get; private set; }
    public EnemyAIState AIState { get; private set; }
    public string Intent => intentValueText.text;

    private void Awake()
    {
        Anim = animatorBehaviour as ICombatantAnimator;

        if (animatorBehaviour != null && Anim == null)
            Debug.LogError($"[{name}] animatorBehaviour does not implement ICombatantAnimator", this);
    }

    public void Setup(EnemyData enemyData)
    {
        AttackValue = enemyData.AttackValue;
        MultiAttackValue = enemyData.MultiAttackValue;
        BlockValue = enemyData.BlockValue;
        BurnValue = enemyData.BurnValue;
        PoisonValue = enemyData.PoisonValue;

        StrengthValue = enemyData.StrengthValue;
        WeakValue = enemyData.WeakValue;



        Behaviour = enemyData.Behaviour;
        AIState = new EnemyAIState(GetInstanceID());

        SetupBase(enemyData.Health, enemyData.Image);
        Anim?.SetIdle();

        ChooseNextIntent();
    }

    public void ChooseNextIntent()
    {
        if (Behaviour == null || AIState == null)
        {
            SetIntentUI(default);
            return;
        }

        var move = Behaviour.PickNextMove(AIState, this);
        AIState.SetMove(move);

        var intent = move != null ? move.GetIntent(this) : default;
        SetIntentUI(intent);
    }

    public List<GameAction> BuildActionsFromCurrentIntent()
    {
        var move = AIState?.CurrentMove;
        if (move == null) return new List<GameAction>();
        return move.BuildActions(this) ?? new List<GameAction>();
    }

    private void SetIntentUI(IntentData intent)
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

