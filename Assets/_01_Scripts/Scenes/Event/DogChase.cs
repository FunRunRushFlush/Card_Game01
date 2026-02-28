using UnityEngine;
using DG.Tweening;
using Game.Scenes.Core;

public class DogChase : MonoBehaviour
{
    [Header("Dog")]
    [SerializeField] private GameObject dog;
    [SerializeField] private float moveSpeedDog = 2f;

    [Header("Hero")]
    [SerializeField] private GameObject hero;
    [SerializeField] private float moveSpeedHero = 4f;

    [Header("Chase Delay")]
    [SerializeField] private float delayTiming = 1f;
    [Header("Trigger")]
    [SerializeField] private GameObject triggerGate;


    private bool heroMoves;
    private bool switched;

    private void Start()
    {
        if (dog == null || hero == null || triggerGate == null) return;

        heroMoves = false;
        Invoke(nameof(StartHero), delayTiming);
    }

    private void StartHero() => heroMoves = true;

    private void Update()
    {
        dog.transform.position += Vector3.right * moveSpeedDog * Time.deltaTime;

        if (heroMoves)
            hero.transform.position += Vector3.right * moveSpeedHero * Time.deltaTime;

        if (!switched && hero.transform.position.x >= triggerGate.transform.position.x)
        {
            switched = true;
            SwitchScene();
        }
    }

    private void SwitchScene()
    {

        //hero.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);


        GameFlowController.Current.EventComplete();
    }
}