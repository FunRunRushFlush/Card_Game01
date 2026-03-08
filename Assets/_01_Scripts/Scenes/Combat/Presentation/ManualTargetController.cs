using UnityEngine;

public class ManualTargetController : Singleton<ManualTargetController>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;

    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetupArrow(startPosition);
    }

    public CombatantId? EndTargeting(Vector3 endPosition)
    {
        arrowView.gameObject.SetActive(false);

        if (Physics.Raycast(endPosition, Vector3.forward, out RaycastHit hit, 20f, targetLayerMask)
            && hit.collider != null
            && hit.transform.TryGetComponent(out EnemyView enemyView))
        {
            return enemyView.Id;
        }

        return null;
    }

    public void CancelTargeting()
    {
        arrowView.gameObject.SetActive(false);
    }
}