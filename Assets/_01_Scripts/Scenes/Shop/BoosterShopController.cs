using Game.Scenes.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterShopController : MonoBehaviour
{
    [SerializeField] private Button jungleButton;
    [SerializeField] private Button fireButton;
    [SerializeField] private Button galaxyButton;


    [Header("Booster Pack Price")]
    [SerializeField] private int jungleBoosterPrice = 75;
    [SerializeField] private int fireBoosterPrice = 100;
    [SerializeField] private int galaxyBoosterPrice = 125;

    [Header("UI")]
    [SerializeField] private Button confirmButton;

    [Header("Selection")]
    [SerializeField] private MultiSelectionGroup selectionGroup;
    [SerializeField] private int pickCount = 1;


    [Header("CardRewardsUI")]
    [SerializeField] private CardRewardsUI cardRewardsUI;
    [SerializeField] private GameObject cardRewardsUIContainer;

    

    [SerializeField] private GameObject[] gameObjectsToDeaktivateAfterPick;

    private bool choiceMade;
    private bool _claimed;

    private List<CardData> generatedCards;

    public void SelectJungle() => Select(BiomeType.Forest, jungleButton);
    public void SelectFire() => Select(BiomeType.Fire, fireButton);
    public void SelectGalaxy() => Select(BiomeType.Galaxy, galaxyButton);

    private void Select(BiomeType biome, Button clicked)
    {

        if (!TryPayForBooster(biome))
            return;

        cardRewardsUIContainer.SetActive(true);
        if (choiceMade)
            return;
        choiceMade = true;

        SetButtonsAfterChoice(clicked);

        var session = CoreManager.Instance?.Session;
        if (session?.Run == null)
            return;

        generatedCards ??= session.RewardSystem.GenerateCardChoicesForBiome(biome);

        cardRewardsUI.CreateRewardViewsUI(generatedCards);

    }

    private void SetButtonsAfterChoice(Button clicked)
    {
        if (jungleButton) jungleButton.interactable = false;
        if (fireButton) fireButton.interactable = false;
        if (galaxyButton) galaxyButton.interactable = false;


        if (clicked) 
            clicked.interactable = true;
    }

    private bool TryPayForBooster(BiomeType biome)
    {
        var session = CoreManager.Instance?.Session;
        var run = session?.Run;
        if (run == null) return false;

        int price = GetPrice(biome);

        if (run.Gold < price)
        {
            // TODO: Error-Sound / UI Feedback "Nicht genug Gold"
            return false;
        }

        run.ChangeAmountOfGold(-price);
        return true;
    }

    public void ResetChoice()
    {
        choiceMade = false;
        if (jungleButton) jungleButton.interactable = true;
        if (fireButton) fireButton.interactable = true;
        if (galaxyButton) galaxyButton.interactable = true;

        cardRewardsUI.ConsumeRewardsUI();
    }
    private int GetPrice(BiomeType biome)
    {
        return biome switch
        {
            BiomeType.Forest => jungleBoosterPrice,
            BiomeType.Fire => fireBoosterPrice,
            BiomeType.Galaxy => galaxyBoosterPrice,
            _ => throw new ArgumentOutOfRangeException(nameof(biome), biome, null)
        };
    }


    private void Awake()
    {
        if (selectionGroup != null)
            selectionGroup.SetRules(pickCount, canDeselect: true);
    }

    private void OnEnable()
    {
        _claimed = false;

        if (selectionGroup != null)
            selectionGroup.SelectionChanged += OnSelectionChanged;

        OnSelectionChanged();
    }

    private void OnDisable()
    {
        if (selectionGroup != null)
            selectionGroup.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (confirmButton == null || selectionGroup == null) return;
        confirmButton.interactable = !_claimed && (selectionGroup.Selected.Count == pickCount);
    }

    public void ClaimCardReward()
    {
        if (_claimed) return;
        if (selectionGroup == null) return;
        if (selectionGroup.Selected.Count != pickCount) return;

        var session = CoreManager.Instance?.Session;
        if (session == null) return;

        _claimed = true;
        if (confirmButton != null) confirmButton.interactable = false;

        foreach (var view in selectionGroup.Selected)
        {
            var data = view?.Card?.Data;
            if (data != null)
                session.Hero.AddPermanent(data);
        }

        foreach(var ele in gameObjectsToDeaktivateAfterPick)
        {
            if(ele != null)
            {
                ele.SetActive(false);
            }
        }
    }
}