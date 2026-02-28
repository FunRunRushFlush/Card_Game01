using Game.Scenes.Core;
using UnityEngine;

public class MainMenuSceneManager : MonoBehaviour
{

    public void StartSession()
    {

        SceneController.Current
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.HeroSelection)
            .WithOverlay()
            .Perform();
    }

    public void Options()
    {
        // ???
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}