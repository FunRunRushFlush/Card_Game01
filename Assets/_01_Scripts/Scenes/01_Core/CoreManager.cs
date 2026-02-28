using UnityEngine;

namespace Game.Scenes.Core
{
    public class CoreManager : MonoBehaviour
    {
        public static CoreManager Instance { get; private set; }
        public ISession Session { get; private set; }

        public Heros HeroID { get; private set; }

        public void RegisterSession(ISession session) => Session = session;
        public void ClearSession() => Session = null;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetSelectedHeroID(Heros heroID)
        {
            HeroID = heroID;
        }

        void Start()
        {
            // Core Setup for the game
            // Load everything like AudioManagers, SaveSystem, InputControlSystem etc.

            SceneController.Current
                .NewTransition()
                .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
                .WithOverlay()
                .Perform();

        }
    }
}
