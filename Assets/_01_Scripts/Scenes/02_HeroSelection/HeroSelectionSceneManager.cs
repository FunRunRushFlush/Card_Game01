using Game.Scenes.Core;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeroSelectionSceneManager : MonoBehaviour
{
    [Header("Assign PickHero buttons here")]
    [SerializeField] private Button[] heroButtons;

    private bool _picked;

    [Header("Secret Hero 2 Settings")]
    [SerializeField] private int secretRequiredClicks = 4;
    [SerializeField] private float secretTimeWindowSeconds = 1f;

    private int _hero2ClickCount;
    private Coroutine _hero2WindowCoroutine;

    public void PickHeroOne()
    {
        if (_picked) return;
        _picked = true;
        SetButtonsInteractable(false);

        CoreManager.Instance.SetSelectedHeroID(Heros.Hero01);
        GameFlowController.Current.StartNewRun();
    }


    public void PickHeroTwo()
    {
        if (_picked) return;

        RegisterHero2SecretClickAndTryTrigger();
    }

    private void RegisterHero2SecretClickAndTryTrigger()
    {

        if (_hero2WindowCoroutine == null)
            _hero2WindowCoroutine = StartCoroutine(Hero2ClickWindow());

        _hero2ClickCount++;

        if (_hero2ClickCount >= secretRequiredClicks)
        {
            ResetHero2SecretState();

            _picked = true;
            SetButtonsInteractable(false);

            CoreManager.Instance.SetSelectedHeroID(Heros.Hero02);
            GameFlowController.Current.StartNewRun();
        }
    }

    private IEnumerator Hero2ClickWindow()
    {
        yield return new WaitForSeconds(secretTimeWindowSeconds);

        ResetHero2SecretState();
    }

    private void ResetHero2SecretState()
    {
        _hero2ClickCount = 0;

        if (_hero2WindowCoroutine != null)
        {
            StopCoroutine(_hero2WindowCoroutine);
            _hero2WindowCoroutine = null;
        }
    }

    public void BackToMainMenu()
    {
        if (_picked)
            return;

        GameFlowController.Current.BackToMainMenu();
    }

    private void SetButtonsInteractable(bool value)
    {
        if (heroButtons == null) return;

        foreach (var b in heroButtons)
            if (b != null) b.interactable = value;
    }

    private void OnDisable()
    {
        ResetHero2SecretState();
    }
}