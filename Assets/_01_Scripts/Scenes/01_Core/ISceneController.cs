using UnityEngine;
namespace Game.Scenes.Core
{
    public interface ISceneController
    {
        Coroutine ExecutePlan(SceneController.SceneTransitionPlan plan);
        void Load(string sceneName);
        SceneController.SceneTransitionPlan NewTransition();
    }
}