using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsManagerUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    [SerializeField] private Sprite armorSprite, burnSprite, poisonSprite, strengthSprite, weaknessSprite;
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUiDic = new();

    public void UpdateStatusEfectUI(StatusEffectType statusEffectType, int stackCount)
    {
        if (stackCount == 0)
        {
            if (statusEffectUiDic.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = statusEffectUiDic[statusEffectType];
                statusEffectUiDic.Remove(statusEffectType);
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            if (!statusEffectUiDic.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                statusEffectUiDic.Add(statusEffectType, statusEffectUI);
            }
            Sprite sprite = GetSpriteByType(statusEffectType);
            statusEffectUiDic[statusEffectType].Set(sprite, stackCount);
        }

    }

    private Sprite GetSpriteByType(StatusEffectType statusEffectType)
    {
        return statusEffectType switch
        {
            StatusEffectType.ARMOR => armorSprite,
            StatusEffectType.BURN => burnSprite,
            StatusEffectType.POISON => poisonSprite,

            StatusEffectType.STRENGTH => strengthSprite,
            StatusEffectType.WEAKNESS => weaknessSprite,
            _ => null
        };
    }
}
