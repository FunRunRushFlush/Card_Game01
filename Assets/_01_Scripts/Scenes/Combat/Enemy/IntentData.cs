using UnityEngine;

[System.Serializable]
public struct IntentData
{
    public Sprite Icon;
    public bool ShowValue;

    public int Value;

    // optional Text "3×5"
    public string ValueText;

    public static IntentData IconOnly(Sprite icon)
        => new IntentData { Icon = icon, ShowValue = false, Value = 0, ValueText = null };

    public static IntentData IconWithValue(Sprite icon, int value)
        => new IntentData { Icon = icon, ShowValue = true, Value = value, ValueText = null };

    public static IntentData IconWithValueText(Sprite icon, int value, string valueText)
        => new IntentData { Icon = icon, ShowValue = true, Value = value, ValueText = valueText };
}