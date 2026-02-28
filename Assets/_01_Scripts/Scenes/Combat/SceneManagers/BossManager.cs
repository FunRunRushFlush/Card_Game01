using Game.Scenes.Core;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public void BossWon()
    {
        GameFlowController.Current.BossWon();

    }
    public void CombatLost()
    {
        GameFlowController.Current.CombatLost();
    }
}