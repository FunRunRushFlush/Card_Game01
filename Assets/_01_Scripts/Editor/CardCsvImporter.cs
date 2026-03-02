using Game.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CardCsvImporter
{
    private const string CsvPath = "Assets/Data/Cards/cards.csv";

    [MenuItem("Tools/Cards/Import cards.csv")]
    public static void ImportNoCreate()
    {
        if (!File.Exists(CsvPath))
        {
            Log.Error(LogArea.Editor, () => $"CSV not found: {CsvPath}");
            return;
        }

        var lines = File.ReadAllLines(CsvPath, Encoding.UTF8);
        if (lines.Length <= 1)
        {
            Log.Error(LogArea.Editor, () => "CSV has no data rows.");
            return;
        }

        var header = ParseCsvLine(lines[0]);
        var col = BuildColumnMap(header);

        RequireColumn(col, "CardKey");
        RequireColumn(col, "DisplayName");
        RequireColumn(col, "Mana");
        RequireColumn(col, "Rarity");
        RequireColumn(col, "Description");

        // Build lookup: CardKey -> CardData
        // - first choice: CardData.cardKey (if you added it)
        // - fallback: asset.name
        var existing = LoadAllCardsByKeyOrName();

        int updated = 0;
        int skippedMissing = 0;
        int errors = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var row = ParseCsvLine(lines[i]);

            string cardKey = Get(row, col, "CardKey")?.Trim();
            if (string.IsNullOrWhiteSpace(cardKey))
            {
                Log.Warn(LogArea.Editor, () => $"Row {i + 1}: missing CardKey.");
                errors++;
                continue;
            }

            if (!existing.TryGetValue(cardKey, out var card) || card == null)
            {
                Log.Warn(LogArea.Editor, () => $"Card '{cardKey}' not found as CardData asset. Skipping row {i + 1}.");
                skippedMissing++;
                continue;
            }

            string displayName = Get(row, col, "DisplayName") ?? "";
            string manaStr = Get(row, col, "Mana") ?? "0";
            string rarityStr = Get(row, col, "Rarity") ?? "";
            string description = Get(row, col, "Description") ?? "";

            if (!int.TryParse(manaStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int mana))
            {
                Log.Warn(LogArea.Editor, () => $"Row {i + 1} ({cardKey}): invalid Mana '{manaStr}'.");
                errors++;
                continue;
            }

            if (!Enum.TryParse(rarityStr, ignoreCase: true, out CardRarity rarity))
            {
                Log.Warn(LogArea.Editor, () => $"Row {i + 1} ({cardKey}): invalid Rarity '{rarityStr}'.");
                errors++;
                continue;
            }

            // Update fields via SerializedObject (supports both fields and auto-properties)
            var so = new SerializedObject(card);

            // If you have [SerializeField] private string cardKey; keep it in sync (optional)
            var keyProp = so.FindProperty("cardKey");
            if (keyProp != null && keyProp.propertyType == SerializedPropertyType.String)
                keyProp.stringValue = cardKey;

            // DisplayName: either field "DisplayName" or auto-property backing "<DisplayName>k__BackingField"
            SetStringAny(so, displayName, "DisplayName", "<DisplayName>k__BackingField");

            // Description: either field "Description" or backing "<Description>k__BackingField"
            SetStringAny(so, description, "Description", "<Description>k__BackingField");

            // Mana: either field "Mana" or backing "<Mana>k__BackingField"
            SetIntAny(so, mana, "Mana", "<Mana>k__BackingField");

            // Rarity: in your code it's a field named "Rarity"
            SetEnumAny(so, (int)rarity, "Rarity");

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(card);
            updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log.Info(LogArea.Editor, () => $"Done. Updated={updated}, SkippedMissing={skippedMissing}, Errors={errors}");
    }

    private static Dictionary<string, CardData> LoadAllCardsByKeyOrName()
    {
        var dict = new Dictionary<string, CardData>(StringComparer.OrdinalIgnoreCase);

        foreach (var guid in AssetDatabase.FindAssets("t:CardData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (asset == null) continue;

            // 1) cardKey field if present
            string key = null;
            var so = new SerializedObject(asset);
            var keyProp = so.FindProperty("cardKey");
            if (keyProp != null && keyProp.propertyType == SerializedPropertyType.String)
                key = keyProp.stringValue;

            if (!string.IsNullOrWhiteSpace(key))
            {
                if (!dict.ContainsKey(key))
                    dict.Add(key, asset);
            }

            // 2) fallback: asset.name
            var nameKey = asset.name;
            if (!string.IsNullOrWhiteSpace(nameKey) && !dict.ContainsKey(nameKey))
                dict.Add(nameKey, asset);
        }

        return dict;
    }

    private static void RequireColumn(Dictionary<string, int> col, string name)
    {
        if (!col.ContainsKey(name))
            throw new Exception($"[CardCsvImporter] Missing required column '{name}'.");
    }

    private static Dictionary<string, int> BuildColumnMap(List<string> header)
    {
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header.Count; i++)
        {
            var h = header[i]?.Trim();
            if (!string.IsNullOrWhiteSpace(h) && !dict.ContainsKey(h))
                dict.Add(h, i);
        }
        return dict;
    }

    private static string Get(List<string> row, Dictionary<string, int> col, string name)
    {
        if (!col.TryGetValue(name, out int idx)) return null;
        if (idx < 0 || idx >= row.Count) return null;
        return row[idx];
    }

    private static void SetStringAny(SerializedObject so, string value, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            var p = so.FindProperty(name);
            if (p != null && p.propertyType == SerializedPropertyType.String)
            {
                p.stringValue = value ?? "";
                return;
            }
        }
    }

    private static void SetIntAny(SerializedObject so, int value, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            var p = so.FindProperty(name);
            if (p != null && p.propertyType == SerializedPropertyType.Integer)
            {
                p.intValue = value;
                return;
            }
        }
    }

    private static void SetEnumAny(SerializedObject so, int enumIndex, params string[] propNames)
    {
        foreach (var name in propNames)
        {
            var p = so.FindProperty(name);
            if (p != null && p.propertyType == SerializedPropertyType.Enum)
            {
                p.enumValueIndex = enumIndex;
                return;
            }
        }
    }

    // Minimal CSV parser that supports quoted fields with commas and quotes ("")
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null) return result;

        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Escaped quote
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == ',')
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}