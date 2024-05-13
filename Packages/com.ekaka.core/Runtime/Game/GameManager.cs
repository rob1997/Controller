using System;
using Core.Common;
using UnityEngine;

namespace Core.Game
{
    //run before everything else
    [DefaultExecutionOrder(- 1)]
    public class GameManager : Singleton<GameManager>
    {
        public const string StudioPrefix = "Ekaka";

        public static bool Initialized => Instance != null && Instance.IsReady;
        
        public bool IsReady { get; private set; }

        private void InvokeReady()
        {
            if (IsReady)
            {
                Debug.LogWarning($"{nameof(GameManager)} already ready");

                return;
            }

            EventBus<GameManagerReady>.Invoke();

            IsReady = true;
        }

        // Add new GameStates here.
        private IGameState[] AllStates => new IGameState[]{
            new Loading(),
            new Landing(),
            new Play(),
            new Pause(),
            new GameOver(),
            new Quitting()
        };

        private int _currentStateIndex = - 1;
        
        [field: SceneList] [field: SerializeField] public int LandingScene { get; private set; }
        
        [field: SceneList] [field: SerializeField] public int GameScene { get; private set; }

        public IGameState CurrentState => _currentStateIndex < 0 ? null : AllStates[_currentStateIndex];

        public bool InGame => CurrentState is IInGameState;
        
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            //all other managers are already initialized OnEnable
            InvokeReady();

            // use for loop since states are mostly structs.
            for (int i = 0; i < AllStates.Length; i++)
            {
                AllStates[i].Initialize();
            }
            
            //now it has finished initializing change state to loading - loading landing scene
            ChangeGameState<Loading>();
            
            //load first/landing scene
            Utils.LoadScene(LandingScene, ChangeGameState<Landing>);
        }

        private void ChangeGameState<T>() where T : IGameState
        {
            if (!AllStates.FindIndex( s => s is T, out int index))
            {
                Debug.LogError($"{nameof(IGameState)} of type {typeof(T).Name} not found.");
                
                return;
            }
            
            if (_currentStateIndex == index)
            {
                Debug.LogWarning($"{nameof(IGameState)} already {CurrentState}.");
                
                return;
            }

            IGameState newState = AllStates[index];
            
            if (!newState.IsReady)
            {
                Debug.LogWarning($"{typeof(T).Name} {nameof(IGameState)} is not ready.");
                
                return;
            }

            // Disable previous state.
            if (_currentStateIndex != - 1)
            {
                AllStates[_currentStateIndex].Disable();
            }
            
            Debug.Log($"{nameof(IGameState)} changed from {CurrentState.TypeName()} to {newState.TypeName()}.");
            
            _currentStateIndex = index;
            
            newState.Enable();
            
            // Update current state reference since states can be structs.
            AllStates[_currentStateIndex] = newState;
            
            EventBus<GameStateEnabled<T>>.Invoke();
            
            EventBus<GameStateChanged>.Invoke(new GameStateChanged(CurrentState));
        }
        
        public void StartGame(bool continued)
        {
            if (continued)
            {
                LoadSavedGame();
            }

            else
            {
                LoadNewGame();
            }
        }

        //load persistent data first
        private void LoadSavedGame(bool tryAgain = false)
        {
            //change to loading until scene loads async
            ChangeGameState<Loading>();
            
            Debug.Log("Loading Saved Game...");
            
            //load game scene and call onSceneLoaded
            Utils.LoadScene(GameScene, GameStarted, tryAgain);
        }

        private void LoadNewGame()
        {
            //change to loading until scene loads async
            ChangeGameState<Loading>();
            
            Debug.Log("Loading New Game...");
            
            //load game scene and call onSceneLoaded
            Utils.LoadScene(GameScene, GameStarted);
        }

        //when game scene finished loading
        void GameStarted()
        {
            ChangeGameState<Play>();
                
            Debug.Log("Loaded New Game");
        }
        
        public void ExitGame()
        {
            ChangeGameState<Quitting>();
        }

        //leave/unload game scene and load to Landing scene
        public void ExitToMainMenu()
        {
            if (!InGame)
            {
                Debug.LogError($"can't exit when {nameof(CurrentState)} is not an {nameof(IInGameState)}.");
                
                return;
            }
            
            //change to loading until scene loads async
            ChangeGameState<Loading>();
            
            Debug.Log($"exiting {nameof(GameScene)}...");
                    
            //load landing scene and call onSceneLoaded
            Utils.LoadScene(LandingScene, GameExited);
        }

        public void TryAgain()
        {
            LoadSavedGame(true);
        }
        
        //called once landing scene is loaded
        private void GameExited()
        {
            Debug.Log($"Exited {nameof(GameScene)}.");
                
            ChangeGameState<Landing>();
        }
        
        public void PauseGame()
        {
            ChangeGameState<Pause>();
        }
        
        public void GameOver()
        {
            ChangeGameState<GameOver>();
        }

        public void ResumeGame()
        {
            ChangeGameState<Play>();
        }
        
        private void OnApplicationQuit()
        {
            if (!(CurrentState is Quitting))
            {
                ChangeGameState<Quitting>();
            }
        }
    }
}