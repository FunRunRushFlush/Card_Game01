using Game.Scenes.Core;
using TMPro;
using UnityEngine;

public class GoldText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    void Update()
    {
        _text.text = $"{CoreManager.Instance.Session.Run.Gold}";
    }
}
