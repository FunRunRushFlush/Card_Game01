using System.Collections.Generic;
using System.Linq;

public sealed class CardPlayabilityResult
{
    public List<CardPlayFailReason> Reasons { get; } = new();

    public bool CanPlay => Reasons.Count == 0;

    public void AddReason(CardPlayFailCode code, string message)
        => Reasons.Add(new CardPlayFailReason(code, message));

    public string TooltipText()
        => string.Join("\n", Reasons.Select(r => r.Message));
}