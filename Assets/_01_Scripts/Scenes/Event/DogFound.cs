using UnityEngine;

public class DogFound : MonoBehaviour
{
    [Header("Dog")]
    [SerializeField] private GameObject dog;
    [SerializeField] private float moveSpeedDog = 2f;
    [SerializeField] private Animator dogAnimator;

    [Header("Hero")]
    [SerializeField] private GameObject hero;
    [SerializeField] private float moveSpeedHero = 4f;
    [SerializeField] private Animator heroAnimator;

    [SerializeField] private float delayTiming = 1f;

    [Header("Targets")]
    [SerializeField] private Transform dogTarget;
    [SerializeField] private Transform heroTarget;

    [Header("Stop Settings")]
    [SerializeField] private float stopDistance = 0.05f;

    private bool dogMoves;
    private bool heroMoves;

    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    private void Start()
    {
        if (dog == null || hero == null) return;


        if (dogAnimator == null) 
            dogAnimator = dog.GetComponent<Animator>();
        if (heroAnimator == null) 
            heroAnimator = hero.GetComponent<Animator>();

        dogMoves = true;
        SetRunning(dogAnimator, true);

        Invoke(nameof(StartHero), delayTiming);
    }

    private void StartHero()
    {
        heroMoves = true;
        SetRunning(heroAnimator, true);
    }

    private void Update()
    {
        if (dogMoves)
            dogMoves = MoveToTarget(dog.transform, dogTarget, moveSpeedDog, dogAnimator);

        if (heroMoves)
            heroMoves = MoveToTarget(hero.transform, heroTarget, moveSpeedHero, heroAnimator);
    }

    private bool MoveToTarget(Transform obj, Transform target, float speed, Animator anim)
    {
        if (target == null)
        {

            obj.position += Vector3.right * speed * Time.deltaTime;
            return true;
        }

        float dist = Vector3.Distance(obj.position, target.position);
        if (dist <= stopDistance)
        {

            obj.position = target.position;
            SetRunning(anim, false);
            return false;
        }

        obj.position = Vector3.MoveTowards(obj.position, target.position, speed * Time.deltaTime);
        return true;
    }

    private void SetRunning(Animator anim, bool running)
    {
        if (anim == null) return;
        anim.SetBool(IsRunning, running); // Animator braucht bool Parameter "IsRunning"
    }
}