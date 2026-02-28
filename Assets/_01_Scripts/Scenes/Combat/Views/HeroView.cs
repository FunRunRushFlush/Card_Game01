using UnityEngine;

public class HeroView : CombatantView
{
    [Header("Animation")]
    [SerializeField] private MonoBehaviour animatorBehaviour;
    public ICombatantAnimator Anim { get; private set; }

    private void Awake()
    {
        Anim = animatorBehaviour as ICombatantAnimator;

        if (animatorBehaviour != null && Anim == null)
            Debug.LogError($"[{name}] animatorBehaviour does not implement ICombatantAnimator", this);
    }

    public void Setup(HeroData heroData)
    {
        SetupBase(heroData.Health, heroData.Image);
        Anim?.SetIdle();
    }
}
