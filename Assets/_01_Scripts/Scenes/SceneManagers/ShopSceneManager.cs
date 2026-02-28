using Game.Scenes.Core;
using UnityEngine;

public class ShopSceneManager : MonoBehaviour
{

    public void BuyItem(int cost)
    {
        //Shop/BuySystem logic ...

        //Dummy logic
        if (cost <= CoreManager.Instance.Session.Run.Gold)
        {

            CoreManager.Instance.Session.Run.ChangeAmountOfGold(cost * -1);
        }

    }

    public void LeaveShop()
    {
        GameFlowController.Current.ShopLeave();
    }
}