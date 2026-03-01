using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class CardTextFormatter
{
    private readonly KeywordDatabase db;
    private readonly bool emitLinks;

    // Each rule matches: optional number + whitespace + keyword (whole word)
    private readonly List<Rule> rules = new();

    // Cache: raw -> formatted
    private readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);

    public CardTextFormatter(KeywordDatabase db, bool emitLinks = true)
    {
        this.db = db;
        this.emitLinks = emitLinks;

        BuildRules();
    }

    public string Format(string raw)
    {
        if (string.IsNullOrEmpty(raw) || db == null) return raw;

        if (cache.TryGetValue(raw, out var cached))
            return cached;

        var result = ApplyRulesAvoidingTags(raw);
        cache[raw] = result;
        return result;
    }

    private void BuildRules()
    {
        rules.Clear();
        if (db == null) return;

        foreach (var def in db.Keywords)
        {
            if (def == null) continue;

            // Build list of match terms (aliases)
            var uniqueTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(def.displayName))
                uniqueTerms.Add(def.displayName.Trim());

            if (!string.IsNullOrWhiteSpace(def.id))
                uniqueTerms.Add(def.id.Trim());

            if (def.aliases != null)
            {
                foreach (var a in def.aliases)
                    if (!string.IsNullOrWhiteSpace(a))
                        uniqueTerms.Add(a.Trim());
            }

            foreach (var term in uniqueTerms)
            {
                // Whole-word keyword, with an OPTIONAL integer directly before it:
                // "8 Block", "+2 Poison", "-1 Strength"
                //
                // (?:(?<num>[+\-]?\d+)\s+)?  -> optional number + at least one whitespace
                // \bTERM\b                    -> whole-word keyword
                var pattern = $@"(?:(?<num>[+\-]?\d+)\s+)?\b{Regex.Escape(term)}\b";

                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                rules.Add(new Rule
                {
                    Regex = regex,
                    Def = def
                });
            }
        }

        // Prefer longer terms if they start at same position, reduces weird overlaps
        rules.Sort((a, b) => b.Regex.ToString().Length.CompareTo(a.Regex.ToString().Length));
    }

    private string ApplyRulesAvoidingTags(string raw)
    {
        // We avoid touching existing TMP tags by splitting on '<...>'
        var sb = new StringBuilder(raw.Length + 32);

        int i = 0;
        while (i < raw.Length)
        {
            int tagStart = raw.IndexOf('<', i);
            if (tagStart == -1)
            {
                sb.Append(ApplyToPlainSegment(raw.Substring(i)));
                break;
            }

            if (tagStart > i)
            {
                sb.Append(ApplyToPlainSegment(raw.Substring(i, tagStart - i)));
            }

            int tagEnd = raw.IndexOf('>', tagStart);
            if (tagEnd == -1)
            {
                // malformed tag -> treat rest as plain
                sb.Append(ApplyToPlainSegment(raw.Substring(tagStart)));
                break;
            }

            // copy tag verbatim
            sb.Append(raw.Substring(tagStart, tagEnd - tagStart + 1));
            i = tagEnd + 1;
        }

        return sb.ToString();
    }

    private string ApplyToPlainSegment(string segment)
    {
        if (string.IsNullOrEmpty(segment) || rules.Count == 0)
            return segment;

        var matches = new List<MatchInfo>(32);

        for (int r = 0; r < rules.Count; r++)
        {
            var rule = rules[r];
            foreach (Match m in rule.Regex.Matches(segment))
            {
                if (!m.Success) continue;

                matches.Add(new MatchInfo
                {
                    Start = m.Index,
                    Length = m.Length,
                    RuleIndex = r,
                    Match = m
                });
            }
        }

        if (matches.Count == 0)
            return segment;

        matches.Sort((a, b) =>
        {
            int cmp = a.Start.CompareTo(b.Start);
            if (cmp != 0) return cmp;
            // prefer longer match if same start
            return b.Length.CompareTo(a.Length);
        });

        var sb = new StringBuilder(segment.Length + 32);

        int cursor = 0;
        foreach (var mi in matches)
        {
            int start = mi.Start;
            int end = mi.Start + mi.Length;

            if (start < cursor)
                continue; // overlap -> skip

            if (start > cursor)
                sb.Append(segment, cursor, start - cursor);

            var rule = rules[mi.RuleIndex];
            sb.Append(FormatMatch(mi.Match, rule.Def));

            cursor = end;
        }

        if (cursor < segment.Length)
            sb.Append(segment, cursor, segment.Length - cursor);

        return sb.ToString();
    }

    private string FormatMatch(Match m, KeywordDefinition def)
    {
        // m.Value is either "Block" or "8 Block" (if number matched)
        // We color the whole matched text, but keep link key stable.
        var shown = m.Value;

        var hex = ColorUtility.ToHtmlStringRGB(def.color);
        var styled = $"<b><color=#{hex}>{shown}</color></b>";

        // Tooltip-ready: later you can hook TMP link events for "kw:<id>"
        if (!emitLinks || string.IsNullOrWhiteSpace(def.id))
            return styled;

        return $"<link=\"kw:{def.id}\">{styled}</link>";
    }

    private struct Rule
    {
        public Regex Regex;
        public KeywordDefinition Def;
    }

    private struct MatchInfo
    {
        public int Start;
        public int Length;
        public int RuleIndex;
        public Match Match;
    }
}