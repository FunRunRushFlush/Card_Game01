using DG.Tweening;
using Game.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    [SerializeField] private CombatantViewRegistry viewRegistry;

    public List<EnemyView> EnemyViews { get; private set; } = new();

    public void AddEnemy(EnemyData enemyData, CombatantId id)
    {
        var slot = GetFirstFreeSlot();
        if (slot == null)
        {
            Log.Warn(LogArea.Combat, () => "No free enemy slot");
            return;
        }

        var enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        enemyView.transform.SetParent(slot, worldPositionStays: true);

        enemyView.AssignId(id);
        viewRegistry?.Register(enemyView);

        EnemyViews.Add(enemyView);
    }

    private Transform GetFirstFreeSlot()
    {
        foreach (var s in slots)
        {
            if (s.childCount == 0)
                return s;
        }
        return null;
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();

        Destroy(enemyView.gameObject);
    }
}