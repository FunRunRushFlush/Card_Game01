using Game.Scenes.Core;
using TMPro;
using UnityEngine;

public class NodeText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    void Update()
    {
        _text.text = $"Biome: {CoreManager.Instance.Session.Run.BiomeIndex} Node: {CoreManager.Instance.Session.Run.NodeIndexInBiome}";
    }
}
