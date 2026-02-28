using Game.Scenes.Core;
using System.Collections;
using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private bool openOnlyOnce = true;
    [SerializeField] private float openDuration = 1.2f; 

    private bool isOpen;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnMouseDown()
    {
        Debug.Log("CHEST CLICKED: " + gameObject.name);
        StartCoroutine(TryOpen());
    }

    private IEnumerator TryOpen()
    {
        if (animator == null) yield break;
        if (openOnlyOnce && isOpen) yield break;

        isOpen = true;
        animator.SetTrigger(openTriggerName);

        yield return new WaitForSeconds(openDuration);

        gameObject.SetActive(false);
        GameFlowController.Current.CombatWon();
    }
}
