using System.Collections;
using UnityEngine;

public class Dog : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animator state names")]
    [SerializeField] private string idleState = "Greate-Dane_Idle";
    [SerializeField] private string aState = "Greate-Dane_Sitting";
    [SerializeField] private string bState = "Greate-Dane_Itching";

    [Header("Timing")]
    [SerializeField] private float minDelay = 5f;
    [SerializeField] private float maxDelay = 15f;
    [SerializeField] private float crossFade = 0.1f;

    private Coroutine loop;

    private void Reset() => animator = GetComponent<Animator>();

    private void OnEnable() => loop = StartCoroutine(Loop());

    private void OnDisable()
    {
        if (loop != null) 
            StopCoroutine(loop);
    }

    private IEnumerator Loop()
    {
        animator.CrossFade(idleState, 0f);
        yield return null;

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            yield return PlayAndWait(aState);
            yield return PlayAndWait(bState);
            yield return PlayAndWait(aState);

            animator.CrossFade(idleState, crossFade);
            yield return null;
        }
    }

    private IEnumerator PlayAndWait(string stateName)
    {
        animator.CrossFade(stateName, crossFade);
        yield return null;


        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;


        while (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
    }
}