using Core.Game;
using Core.Input;

namespace Core.Common
{
    public struct ManagerReady<T> : IEventParams where T : Manager<T>
    {
    }
    
    public struct GameManagerReady : IEventParams
    {
    }
    
    public struct GameStateEnabled<T> : IEventParams where T : IGameState
    {
    }
    
    public struct GameStateChanged : IEventParams
    {
        public IGameState State { get; private set; }
        
        public GameStateChanged(IGameState state)
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