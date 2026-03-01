using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CardCsvExporter
{
    private const string DefaultExportPath = "Assets/Data/Cards/cards_export.csv";

    [MenuItem("Tools/Cards/Export CardData to CSV")]
    public static void Export()
    {
        Directory.CreateDirectory("Assets/Data/Cards");

        var guids = AssetDatabase.FindAssets("t:CardData");
        var rows = new List<string>(guids.Length + 1);

        // Header
        rows.Add("CardKey,DisplayName,Mana,Rarity,Description,AssetGuid");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var card = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card == null) continue;

            // Try to read cardKey if it exists, otherwise fallback to asset name
            string cardKey = TryGetCardKey(card);
            if (string.IsNullOrWhiteSpace(cardKey))
                cardKey = card.name;

            // Read fields via SerializedObject to be resilient to your private fields/backing fields
            var so = new SerializedObject(card);

            string displayName = ReadStringAny(so, "DisplayName", "<DisplayName>k__BackingField") ?? "";
            int mana = ReadIntAny(so, "Mana", "<Mana>k__BackingField");
            string description = ReadStringAny(so, "Description", "<Description>k__BackingField") ?? "";

            // Rarity is a field on your CardData (public CardRarity Rarity = ...)
            // We can safely read it directly if it’s public; but we read via SerializedObject anyway.
            string rarity = ReadEnumName(so, "Rarity") ?? card.Rarity.ToString();

            rows.Add(string.Join(",",
                Csv(cardKey),
                Csv(displayName),
                mana.ToString(),
                Csv(rarity),
                Csv(description),
                Csv(guid) // asset GUID for debugging/reference
            ));
        }

        // Sort by CardKey for stable diffs
        rows.Sort(1, rows.Count - 1, Comparer<string>.Create((a, b) =>
        {
            // Compare by first column (CardKey)
            var ak = a.Split(',')[0];
            var bk = b.Split(',')[0];
            return string.Compare(ak, bk, StringComparison.OrdinalIgnoreCase);
        }));

        File.WriteAllLines(DefaultExportPath, rows, Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log($"[CardCsvExporter] Exported {guids.Length} CardData assets to: {DefaultExportPath}");
    }

    private static string TryGetCardKey(CardData card)
    {
        // If you added:
        // [SerializeField] private string cardKey;
        // public string CardKey => cardKey;
        //
        // We can read it via SerializedObject even if it's private.
        var so = new SerializedObject(card);
        var p = so.FindProperty("cardKey");
        return p != null && p.propertyType == SerializedPropertyType.String ? p.stringValue : null;
    }

    private static string ReadString(SerializedObject so, string propName)
    {
        var p = so.FindProperty(propName);
        if (p == null || p.propertyType != SerializedPropertyType.String) return null;
        return p.stringValue;
    }

    private static int ReadInt(SerializedObject so, string propName)
    {
        var p = so.FindProperty(propName);
        if (p == null || p.propertyType != SerializedPropertyType.Integer) return 0;
        return p.intValue;
    }

    private static string ReadEnumName(SerializedObject so, string propName)
    {
        var p = so.FindProperty(propName);
        if (p == null || p.propertyType != SerializedPropertyType.Enum) return null;
        if (p.enumNames == null || p.enumNames.Length == 0) return null;
        var idx = Mathf.Clamp(p.enumValueIndex, 0, p.enumNames.Length - 1);
        return p.enumNames[idx];
    }

    private static string Csv(string value)
    {
        if (value == null) return "";
        // Escape quotes by doubling them; wrap in quotes if contains comma, quote, newline
        bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
        if (!mustQuote) return value;

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
    private static string ReadStringAny(SerializedObject so, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            var p = so.FindProperty(name);
            if (p != null && p.propertyType == SerializedPropertyType.String)
                return p.stringValue;
        }
        return null;
    }

    private static int ReadIntAny(SerializedObject so, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            var p = so.FindProperty(name);
            if (p != null && p.propertyType == SerializedPropertyType.Integer)
                return p.intValue;
        }
        return 0;
    }
}