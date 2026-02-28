using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeWidgetUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    [Header("Icons")]
    [SerializeField] private Sprite combatIcon;
    [SerializeField] private Sprite eliteIcon;
    [SerializeField] private Sprite shopIcon;
    [SerializeField] private Sprite eventIcon;
    [SerializeField] private Sprite bossIcon;

    [Header("Colors")]
    [SerializeField] private Color currentColor = Color.white;
    [SerializeField] private Color pastColor = new Color(1, 1, 1, 0.6f);
    [SerializeField] private Color futureColor = new Color(1, 1, 1, 0.25f);

    public void SetNode(int globalIndex, BiomeType biome, MapNodeType type)
    {
        if (label) label.text = $"{globalIndex + 1}\n{type}";
        if (icon) icon.sprite = GetIcon(type);
    }

    public void SetState(bool isCurrent, bool isPast)
    {
        if (isCurrent)
        {
            Apply(currentColor, highlight: true);
        }
        else if (isPast)
        {
            Apply(pastColor, highlight: false);
        }
        else
        {
            Apply(futureColor, highlight: false);
        }
    }

    private void Apply(Color c, bool highlight)
    {
        if (label) label.color = c;
        if (icon) icon.color = c;

        if (background)
        {
            background.enabled = true;
            background.color = new Color(c.r, c.g, c.b, highlight ? 0.25f : 0.08f);
            // Optional: scale bump on current
            transform.localScale = highlight ? Vector3.one * 1.1f : Vector3.one;
        }
    }

    private Sprite GetIcon(MapNodeType t) => t switch
    {
        MapNodeType.Combat => combatIcon,
        MapNodeType.EliteCombat => eliteIcon,
        MapNodeType.Shop => shopIcon,
        MapNodeType.Event => eventIcon,
        MapNodeType.Boss => bossIcon,
        _ => combatIcon
    };
}
