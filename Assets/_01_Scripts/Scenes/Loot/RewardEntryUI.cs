using UnityEngine;


public class RewardEntryUI : MonoBehaviour
{
    [SerializeField] private RewardPopUpCanvasUI popup;

    [Tooltip("Optional. If set, this GameObject will be destroyed when the reward is claimed. If null, the current GameObject is destroyed.")]
    [SerializeField] private GameObject rootToRemove;

    private bool _claimed;

    public bool IsClaimed => _claimed;

    public void Open()
    {
        if (_claimed) return;
        popup?.ShowRewardPopUpCanvas();
    }

    /// <summary>
    /// Marks this reward as claimed and removes the entry from the rewards list.
    /// Call this after the player successfully recieved the reward.
    /// </summary>
    public void MarkClaimedAndRemove()
    {
        if (_claimed) return;
        _claimed = true;

        var target = rootToRemove != null ? rootToRemove : gameObject;
        Destroy(target);
    }
}
