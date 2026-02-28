using Game.Scenes.Core;
using System.Collections.Generic;
using UnityEngine;


public class RunDeck : MonoBehaviour
{

    [Header("Hero Start Decks")]
    [SerializeField] private HeroData hero01Data;
    [SerializeField] private HeroData hero02Data;


    private List<CardData> deck = new();

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
    }


    public List<CardData> CreateCombatSnapshot()
    {
        return new List<CardData>(deck);
    }

    public void AddPermanent(CardData card) => deck.Add(card);
    public void RemovePermanent(CardData card) => deck.Remove(card);
}