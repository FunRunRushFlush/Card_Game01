using UnityEngine;
using UnityEngine.UI;

public class DisableButtonOnAnyClaim : MonoBehaviour
{
    [Header("Optional: if empty, uses Button on same GameObject")]
    [SerializeField] private Button targetButton;
    [SerializeField] private GameObject targetButtonObject;

    private void Awake()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        ClaimEvents.AnyClaimed += OnAnyClaimed;
    }

    private void OnDisable()
    {
        ClaimEvents.AnyClaimed -= OnAnyClaimed;
    }

    private void OnAnyClaimed()
    {
        if (targetButton == null)
            return;

        targetButton.interactable = false;

        if (targetButtonObject == null)
            return;

        targetButtonObject.SetActive(false);
    }
}