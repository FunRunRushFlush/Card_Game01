using UnityEngine;

public class CardGlow : MonoBehaviour
{
    [SerializeField] private GameObject _glowVisual;

    public void SetPlayable(bool playable)
    {
        if (_glowVisual != null)
        {
            _glowVisual.SetActive(playable);
        }
        else
        {
            gameObject.SetActive(playable);
        }
    }
}
