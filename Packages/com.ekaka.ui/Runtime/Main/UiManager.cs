using Core.Game;
using Core.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace Ui.Main
{
    public class UiManager : Manager<UiManager>
    {
        [field: SerializeField] public UiRoot UiRoot { get; private set; }
        
        [field: SerializeField] public UiPreferences UiPreferences { get; private set; }
        
        [field: SerializeField] public UiReferences UiReferences { get; private set; }

        public override void Initialize()
        {
            Debug.Log($"Initializing {nameof(UiManager)}...");
            
            UiRoot.Initialize(this);

            //make sure canvas and ui EventSystem is loaded all the time (even on scene load/unload)
            //only one main canvas and EventSystem in game session
            DontDestroyOnLoad(UiRoot.Canvas.gameObject);
            
            DontDestroyOnLoad(EventSystem.current.gameObject);
            
            //register GameStateChanged events
            EventBus<GameStateChanged>.Subscribe(GameStateChanged);
            
            //register OnSceneLoaded events (loading landing uiMenus)
            EventBus<SceneLoaded>.Subscribe(TryLoadLandingUiMenus);
            
            //load landing uiMenus for first scene
            TryLoadLandingUiMenus(new SceneLoaded(0));
            
            Debug.Log($"{nameof(UiManager)} Initialized");
        }

        private void GameStateChanged(GameStateChanged gameStateChanged)
        {
            IGameState state = gameStateChanged.State;
            
            switch (state)
            {
                case Loading:
                    //when GameState is loading unload all menus, ready for the next GameState
                    if (!UiRoot.AllUiLayersUnloaded) UiRoot.UnloadAllLayers();
                    break;
                
                case Play:
                    //lock cursor on play/in Game
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                    
                case Pause: case GameOver:
                    //unlock cursor on resume/in Game
                    Cursor.lockState = CursorLockMode.None;
                    break;
            }
        }

        /// <summary>
        /// try and load landing UiMenus for a specific scene
        /// </summary>
        /// <param name="sceneLoaded">Loaded scene event params.</param>
        private void TryLoadLandingUiMenus(SceneLoaded sceneLoaded)
        {
            int sceneBuildIndex = sceneLoaded.SceneBuildIndex;
            
            if (UiReferences.LandingUiMenus.TryGetValue(sceneBuildIndex, out var uiMenuTypes))
            {
                foreach (string uiMenuType in uiMenuTypes)
                {
                    UiRoot.LoadUiMenu(uiMenuType);
                }
            }

            else
            {
                Debug.LogError($"{nameof(UiReferences.LandingUiMenus)} doesn't contain {sceneBuildIndex} key/scene");
            }
        }
        
        public bool GetMenuReference(string uiMenuType, out AssetReference menuReference)
        {
            menuReference = null;
            
            if (UiReferences == null)
            {
                return false;
            }

            return UiReferences.GetMenuReference(uiMenuType, out menuReference);
        }
    }
}