using Game.Scenes.Core;
using TMPro;
using UnityEngine;

public class MapeNodeText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    void Update()
    {
        _text.text = $"Node: {CoreManager.Instance.Session.Run.NodeIndexInBiome}";
    }
}
