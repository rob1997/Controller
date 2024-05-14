namespace Core.Game
{
    public interface IGameState
    {
        /// <summary>
        /// Is the game state ready/able to be enabled.
        /// For example pause Game state is only ready when current state is Play state.
        /// </summary>
        public bool IsReady { get; }

        public void Initialize()
        {
        }
        
        public void Enable()
        {
        }

        public void Disable()
        {   
        }
    }

    /// <summary>
    /// In Game states like play, pause and game over.
    /// </summary>
    public interface IInGameState : IGameState
    {
    }
}