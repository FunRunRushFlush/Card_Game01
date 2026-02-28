using Game.Scenes.Core;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    //public void SwitchToCombat()
    //{
    //    GameFlowController.Current.ChooseCombat();
    //}
    //public void SwitchToShop()
    //{
    //    GameFlowController.Current.ChooseShop();
    //}

    //public void SwitchToEvent()
    //{
    //    GameFlowController.Current.ChooseEvent();
    //}

    public void Continue()
    {
        GameFlowController.Current.GoToCurrentNode();
    }

}