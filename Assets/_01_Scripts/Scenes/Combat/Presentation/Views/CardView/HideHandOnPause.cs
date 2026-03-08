using UnityEngine;

public class HideHandOnPause : MonoBehaviour
{
    [SerializeField] private PauseChannel pauseChannel;
    [SerializeField] private HandView handView;

    [Header("Hide HandCardsRoot")]
    [SerializeField] private GameObject handCardContrainer;

    private bool forceHidden; // set true on combat won

    private void Reset()
    {
        handView = GetComponent<HandView>();
        handCardContrainer = gameObject;
    }

    private void OnEnable()
    {
        if (pauseChannel != null)
            pauseChannel.PauseChanged += OnPauseChanged;

        if (EnemySystem.Instance != null)
            EnemySystem.Instance.AllEnemiesDefeated += OnCombatWon;

        ApplyVisibility();
    }

    private void OnDisable()
    {
        if (pauseChannel != null)
            pauseChannel.PauseChanged -= OnPauseChanged;

        if (EnemySystem.Instance != null)
            EnemySystem.Instance.AllEnemiesDefeated -= OnCombatWon;

        // NICHT hier handRoot togglen (sonst "already being activated/deactivated" möglich)
    }

    private void OnPauseChanged(bool _)
    {
        // Wenn gerade pausiert wird: laufende Interaktion sauber abbrechen
        if (pauseChannel != null && pauseChannel.IsPaused)
            CancelActiveInteraction();

        ApplyVisibility();
    }

    private void OnCombatWon()
    {
        forceHidden = true;

        CancelActiveInteraction();
        HideHoverAndTargeting();

        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        bool paused = pauseChannel != null && pauseChannel.IsPaused;
        bool visible = !(paused || forceHidden);

        if (handCardContrainer != null)
        {
            if (handCardContrainer.activeSelf != visible)
                handCardContrainer.SetActive(visible);
        }
    }

    private void CancelActiveInteraction()
    {
        if (handView != null)
            handView.CancelAllDragging();
    }

    private void HideHoverAndTargeting()
    {
        if (CardViewHoverPresentor.Instance != null)
            CardViewHoverPresentor.Instance.Hide();

        if (ManualTargetController.Instance != null)
            ManualTargetController.Instance.CancelTargeting();
    }

    // Optional: wenn du später die Hand wieder anzeigen willst (z.B. beim nächsten Combat-Start)
    public void ClearForceHidden()
    {
        forceHidden = false;
        ApplyVisibility();
    }

    // Optional: manuell setzen
    public void SetForceHidden(bool hidden)
    {
        forceHidden = hidden;
        ApplyVisibility();
    }
}