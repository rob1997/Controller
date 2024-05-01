using Core.Game;
using Core.Input;

namespace Core.Utils
{
    public struct ManagerReady<T> : IEventParams where T : Manager<T>
    {
    }
    
    public struct GameManagerReady : IEventParams
    {
    }
    
    public struct GameStateChanged : IEventParams
    {
        public GameState State { get; private set; }

        public GameStateChanged(GameState state)
        {
            State = state;
        }
    }

    public struct InputActionsInitialized : IEventParams
    {
        public BaseInputActions InputActions { get; private set; }
        
        public InputActionsInitialized(BaseInputActions inputActions)
        {
            InputActions = inputActions;
        }
    }
    
    public struct LoadingScene : IEventParams
    {
        public int SceneBuildIndex { get; private set; }
        
        public LoadingScene(int sceneBuildIndex)
        {
            SceneBuildIndex = sceneBuildIndex;
        }
    }
    
    public struct SceneLoaded : IEventParams
    {
        public int SceneBuildIndex { get; private set; }
        
        public SceneLoaded(int sceneBuildIndex)
        {
            SceneBuildIndex = sceneBuildIndex;
        }
    }
}