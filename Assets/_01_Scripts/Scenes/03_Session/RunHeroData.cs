using Game.Scenes.Core;
using System.Collections.Generic;
using UnityEngine;


public class RunHeroData : MonoBehaviour
{

    [Header("Hero Data")]
    [SerializeField] private HeroData hero01Data;
    [SerializeField] private HeroData hero02Data;
    [SerializeField] private HeroData hero03Data;


    private List<CardData> deck = new();

    private HeroData selectedHero;
    public HeroData Data => selectedHero;

    public IReadOnlyList<CardData> Deck => deck;

    private void Awake()
    {
        var hero = CoreManager.Instance.HeroID;
        var startDeck = hero == Heros.Hero02 ? hero02Data : hero01Data;

        InitializeFromHero(startDeck);
    }


    public void InitializeFromHero(HeroData heroData)
    {
        deck = new List<CardData>(heroData.Deck);
        selectedHero = heroData;
    }

    public List<CardData> CreateCombatSnapshot()
    {
        return new List<CardData>(deck);
    }

    public void AddPermanent(CardData card) => deck.Add(card);
    public void RemovePermanent(CardData card) => deck.Remove(card);
}