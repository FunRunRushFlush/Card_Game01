using Game.Scenes.Core;
using UnityEngine;
using TMPro;

public class EventManager : MonoBehaviour
{
   

    public void ChangeAmountOfGold(int goldAmount)
    {
        CoreManager.Instance.Session.Run.ChangeAmountOfGold(goldAmount);
        EventComplete();
    }


    public void EventComplete()
    {
        GameFlowController.Current.EventComplete();
    }

    public void BackToMainMenu()
    {
        GameFlowController.Current.BackToMainMenu();
    }
}