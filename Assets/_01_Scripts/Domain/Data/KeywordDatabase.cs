using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Keyword Database")]
public class KeywordDatabase : ScriptableObject
{
    [SerializeField] private List<KeywordDefinition> keywords = new();

    public IReadOnlyList<KeywordDefinition> Keywords => keywords;
}

[Serializable]
public class KeywordDefinition
{
    [Tooltip("Stable id like 'burn' or 'damage'.")]
    public string id;

    [Tooltip("What should be displayed in text. If empty, we use the matched word.")]
    public string displayName;

    [Tooltip("Aliases that should also match (case-insensitive).")]
    public List<string> aliases = new();

    public Color color = Color.white;

    [TextArea] public string tooltip; // for later
}