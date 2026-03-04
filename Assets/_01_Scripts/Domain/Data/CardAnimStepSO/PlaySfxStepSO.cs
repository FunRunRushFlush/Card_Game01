using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Card Animation Steps/Play SFX")]
public class PlaySfxStepSO : CardAnimStepSO
{
    [SerializeField] private AudioClip clip;

    public override IEnumerator Play(CardAnimContext ctx)
    {
        // Minimal: play 2D audio from controller (you can replace with your AudioService later)
        if (clip != null && ctx?.Presentation != null)
            ctx.Presentation.PlayOneShot(clip);

        yield break;
    }
}