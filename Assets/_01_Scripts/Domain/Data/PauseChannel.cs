using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Channels/PauseChannel")]
public class PauseChannel : ScriptableObject
{
    public bool IsPaused { get; private set; }
    public event Action<bool> PauseChanged;

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;
        PauseChanged?.Invoke(IsPaused);
    }

    public void Toggle() => SetPaused(!IsPaused);
}