#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Core.Game
{
    /// <summary>
    /// When loading between scenes or game states.
    /// From landing to play from game scene to menu...
    /// </summary>
    public struct Loading : IGameState
    {
        public bool IsReady => true;
    }
    
    /// <summary>
    /// First landing scene (main menu).
    /// </summary>
    public struct Landing : IGameState
    {
        public bool IsReady => GameManager.Instance.CurrentState is Loading;
    }
    
    /// <summary>
    /// In game scene/playing game.
    /// </summary>
    public struct Play : IInGameState
    {
        public bool IsReady
        {
            get
            {
                IGameState state = GameManager.Instance.CurrentState;
                
                return state is Loading || state is Pause;
            }
        }
    }
    
    /// <summary>
    /// When game is paused - still in game scene.
    /// </summary>
    public struct Pause : IInGameState
    {
        // Can only pause from Play state.
        public bool IsReady => GameManager.Instance.CurrentState is Play;

        public void Enable()
        {
            Time.timeScale = 0f;
        }

        public void Disable()
        {
            Time.timeScale = 1f;
        }
    }
    
    /// <summary>
    /// When game is over (Player is dead) - still in game scene.
    /// </summary>
    public struct GameOver : IInGameState
    {
        public bool IsReady => GameManager.Instance.CurrentState is IInGameState;
    }
    
    /// <summary>
    /// OnApplicationQuit (maybe use for dispose/garbage collection).
    /// </summary>
    public class Quitting : IGameState
    {
        public bool IsReady => true;

        private bool _wantToQuit;
        
        public void Initialize()
        {
            Application.wantsToQuit += WantToQuit;
        }
        
        public void Enable()
        {
            if (!_wantToQuit)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private bool WantToQuit()
        {
            _wantToQuit = true;

            return true;
        }
    }
}