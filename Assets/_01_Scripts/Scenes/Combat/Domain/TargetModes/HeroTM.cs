using System.Collections.Generic;

public class HeroTM : TargetMode
{
    public override List<CombatantId> GetTargetIds()
       => new() { CombatantIds.Hero };
}