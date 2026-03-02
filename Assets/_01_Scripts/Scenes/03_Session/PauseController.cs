using Game.Scenes.Core;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] private PauseChannel pauseChannel;
    [SerializeField] private GameObject pauseMenuRoot;

    private float _prevTimeScale = 1f;

    private void Awake()
    {
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (pauseChannel != null)
            pauseChannel.SetPaused(false);
    }

    private void OnEnable()
    {
        if (pauseChannel != null)
            pauseChannel.PauseChanged += OnPauseChanged;
    }

    private void OnDisable()
    {
        if (pauseChannel != null)
            pauseChannel.PauseChanged -= OnPauseChanged;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (pauseChannel != null && Input.GetKeyDown(KeyCode.Escape))
            pauseChannel.Toggle();
    }


    public void TogglePause()
    {
        if (pauseChannel != null)
            pauseChannel.Toggle();
    }

    public void Resume()
    {
        if (pauseChannel != null)
            pauseChannel.SetPaused(false);
    }

    public void QuitRun()
    {
        if (GameFlowController.Current != null)
            GameFlowController.Current.BackToMainMenu();
        if (pauseChannel != null)
            pauseChannel.Toggle();
    }

    private void OnPauseChanged(bool paused)
    {
        if (paused)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = _prevTimeScale <= 0f ? 1f : _prevTimeScale;
        }

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(paused);
    }
}