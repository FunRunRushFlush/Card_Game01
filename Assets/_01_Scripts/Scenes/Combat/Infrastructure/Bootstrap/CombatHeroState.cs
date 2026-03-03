public sealed class CombatHeroState
{
    // Basis (aus Snapshot)
    public int BaseDrawPerTurn { get; private set; }
    public int BaseMaxHandSize { get; private set; }
    public int BaseMaxMana { get; private set; }

    // temporäre Modifikatoren (combat-only)
    public int DrawPerTurnBonus { get; private set; }
    public int MaxHandSizeBonus { get; private set; }
    public int MaxManaBonus { get; private set; }

    // effektive Werte
    public int DrawPerTurn => BaseDrawPerTurn + DrawPerTurnBonus;
    public int MaxHandSize => BaseMaxHandSize + MaxHandSizeBonus;
    public int MaxMana => BaseMaxMana + MaxManaBonus;

    public CombatHeroState(CombatSetupSnapshot snap)
    {
        BaseDrawPerTurn = snap.drawPerTurn;
        BaseMaxHandSize = snap.maxHandSize;
        BaseMaxMana = snap.maxMana;
    }

    // API für Karten/Status
    public void AddDrawPerTurn(int delta) => DrawPerTurnBonus += delta;
    public void AddMaxHandSize(int delta) => MaxHandSizeBonus += delta;
    public void AddMaxMana(int delta) => MaxManaBonus += delta;
}