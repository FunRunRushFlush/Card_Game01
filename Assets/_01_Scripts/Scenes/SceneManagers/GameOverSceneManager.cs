using Game.Scenes.Core;
using UnityEngine;

public class GameOverSceneManager : MonoBehaviour
{
    public void BackToMainMenu()
    {
        GameFlowController.Current.BackToMainMenu();
    }

    public void BuyItem(int goldCost)
    {

    }

}