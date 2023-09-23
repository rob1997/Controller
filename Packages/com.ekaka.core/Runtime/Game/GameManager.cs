using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Core.Game
{
    public enum GameState
    {
        //happens only once when game is first loaded on start
        //used to load assets and initialize GameManager
        Initializing,
        //when loading between scenes or game states
        //from landing to play from game scene to menu...
        Loading,
        //first landing scene (main menu)
        Landing,
        //in game scene/playing game
        Play,
        //when game is paused - still in game scene
        Pause,
        //onApplicationQuit (maybe use for dispose/garbage collection)
        Quitting
    }
    
    //run before everything else
    [DefaultExecutionOrder(- 1)]
    public class GameManager : Singleton<GameManager>
    {
        public const string StudioPrefix = "Ekaka";

        public static bool Initialized => Instance != null && Instance.IsReady;
        
        #region Ready

        public delegate void Ready();

        //all Managers have been initialized
        public event Ready OnReady;

        public bool IsReady { get; private set; }

        private void InvokeReady()
        {
            if (IsReady)
            {
                Debug.LogWarning($"{nameof(GameManager)} already ready");

                return;
            }

            OnReady?.Invoke();

            IsReady = true;
        }

        #endregion

        #region GameStateChanged

        public delegate void GameStateChanged(GameState state);

        public event GameStateChanged OnGameStateChanged;

        private void InvokeGameStateChanged(GameState state)
        {
            OnGameStateChanged?.Invoke(state);
        }

        #endregion

        [field: SceneList] [field: SerializeField] public int LandingScene { get; private set; }
        
        [field: SceneList] [field: SerializeField] public int GameScene { get; private set; }

        public GameState State { get; private set; } = GameState.Initializing;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            //all other managers are already initialized OnEnable
            InvokeReady();
            
            //now it has finished initializing change state to loading - loading landing scene
            ChangeGameState(GameState.Loading);
            
            //load first/landing scene
            Utils.Utils.LoadScene(LandingScene, delegate { ChangeGameState(GameState.Landing); });
        }

        private void ChangeGameState(GameState newState)
        {
            if (newState == State)
            {
                Debug.LogWarning($"{nameof(GameState)} already {State}");
                
                return;
            }

            Debug.Log($"{nameof(GameState)} changed from {State} to {newState}");
            
            State = newState;
            
            InvokeGameStateChanged(State);
        }
        
        public void StartGame(bool continued)
        {
            if (continued)
            {
                ContinueGame();
            }

            else
            {
                StartNewGame();
            }
        }

        //load persistent data first
        private void ContinueGame()
        {
            //implement
        }

        private void StartNewGame()
        {
            //change to loading until scene loads async
            ChangeGameState(GameState.Loading);
            
            Debug.Log("Loading New Game...");
            
            //load game scene and call onSceneLoaded
            Utils.Utils.LoadScene(GameScene, NewGameStarted);
        }

        //when game scene finished loading
        void NewGameStarted()
        {
            ChangeGameState(GameState.Play);
                
            Debug.Log("Loaded New Game");
        }
        
        public void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        //leave/unload game scene and load to Landing scene
        public void ExitToMainMenu()
        {
            switch (State)
            {
                case GameState.Play: case GameState.Pause:
                    
                    //change to loading until scene loads async
                    ChangeGameState(GameState.Loading);
            
                    Debug.Log($"exiting {nameof(GameScene)}...");
                    
                    //load landing scene and call onSceneLoaded
                    Utils.Utils.LoadScene(LandingScene, GameExited);
                    
                    break;
                
                default:
                    Debug.LogError($"can't exit {nameof(GameScene)} when {nameof(GameState)} is {State}");
                    return;
            }
        }
        
        //called once landing scene is loaded
        private void GameExited()
        {
            Debug.Log($"Exited {nameof(GameScene)}");
                
            ChangeGameState(GameState.Landing);
                
            //reset timeScale in case it was exited in pause
            Time.timeScale = 1f;
        }
        
        public void PauseGame()
        {
            //pause only from GameState.Play
            if (State != GameState.Play)
            {
                Debug.LogWarning($"can't {nameof(PauseGame)} when {nameof(GameState)} is {State}");
                
                return;
            }
            
            Time.timeScale = 0f;

            ChangeGameState(GameState.Pause);
        }

        public void ResumeGame()
        {
            //resume only from GameState.Pause
            if (State != GameState.Pause)
            {
                Debug.LogWarning($"can't {nameof(ResumeGame)} when {nameof(GameState)} is {State}");
                
                return;
            }
            
            Time.timeScale = 1f;
            
            ChangeGameState(GameState.Play);
        }
        
        private void OnApplicationQuit()
        {
            ChangeGameState(GameState.Quitting);
        }
    }
}