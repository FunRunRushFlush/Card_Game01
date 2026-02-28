
using Game.Scenes.Core;
using UnityEngine;
using UnityEngine.UI;

public class LootSceneManager : MonoBehaviour
{
    public void LeaveLootScreen()
    {
        GameFlowController.Current?.LootPicked();
    }
}
