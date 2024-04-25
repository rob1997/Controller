using Core.Game;
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
            GameManager.Instance.OnGameStateChanged += GameStateChanged;
            
            //register OnSceneLoaded events (loading landing uiMenus)
            Core.Utils.Utils.OnSceneLoaded += TryLoadLandingUiMenus;
            
            //load landing uiMenus for first scene
            TryLoadLandingUiMenus(0);
            
            Debug.Log($"{nameof(UiManager)} Initialized");
        }

        private void GameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                    //when GameState is loading unload all menus, ready for the next GameState
                    if (!UiRoot.AllUiLayersUnloaded) UiRoot.UnloadAllLayers();
                    break;
                
                case GameState.Play:
                    //lock cursor on play/in Game
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                    
                case GameState.Pause: case GameState.GameOver:
                    //unlock cursor on resume/in Game
                    Cursor.lockState = CursorLockMode.None;
                    break;
            }
        }
        
        /// <summary>
        /// try and load landing UiMenus for a specific scene
        /// </summary>
        /// <param name="sceneBuildIndex">build index for a loaded scene</param>
        private void TryLoadLandingUiMenus(int sceneBuildIndex)
        {
            if (UiReferences.LandingUiMenus.ContainsKey(sceneBuildIndex))
            {
                foreach (string uiMenuType in UiReferences.LandingUiMenus[sceneBuildIndex])
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