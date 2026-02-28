using UnityEngine;

public class ActivateOnClick : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool setActiveTo = true;

    void OnMouseDown()
    {
        if (target != null)
            target.SetActive(setActiveTo);
    }
}