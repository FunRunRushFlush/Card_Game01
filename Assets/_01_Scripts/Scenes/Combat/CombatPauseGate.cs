using UnityEngine;


    public class CombatPauseGateSystem : Singleton<CombatPauseGateSystem>
    {
    [SerializeField] private PauseChannel pauseChannel;

    public bool IsPaused => pauseChannel != null && pauseChannel.IsPaused;
}