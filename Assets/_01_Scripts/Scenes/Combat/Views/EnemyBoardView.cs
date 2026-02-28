using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public void AddEnemy(EnemyData enemyData)
    {
        var slot = GetFirstFreeSlot();
        if (slot == null) { Debug.LogWarning("No free enemy slot"); return; }

        var enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        enemyView.transform.SetParent(slot, worldPositionStays: true);
        EnemyViews.Add(enemyView);
    }

    private Transform GetFirstFreeSlot()
    {
        foreach (var s in slots)
        {
            if (s.childCount == 0)
            {
                return s;
            }
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
