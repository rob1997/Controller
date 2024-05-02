using Core.Game;
using Core.Common;
using UnityEngine.InputSystem;

namespace Core.Input
{
    public class InputManager : Manager<InputManager>
    {
        public BaseInputActions InputActions { get; private set; }

        public override void Initialize()
        {
            //on pause/resume disable/enable input actions
            EventBus<GameStateChanged>.Subscribe(GameStateChanged);
        }

        //enables/disables input based on game state
        private void GameStateChanged(GameStateChanged gameStateChanged)
        {
            GameState state = gameStateChanged.State;
            
            switch (state)
            {
                //loading - when exiting game or before game loads/initializes
                //reinitialize to unsubscribe to all the game actions subscribed during/at the start of Play
                case GameState.Loading:
                    //dispose Input action if it exists and re-initialize
                    InputActions?.Dispose();
                    //initialize
                    InputActions = new BaseInputActions();
                    //enable after re-initializing
                    InputActions.Enable();
                    
                    //invoked when input actions in initialized or re-initialized
                    EventBus<InputActionsInitialized>.Invoke(new InputActionsInitialized(InputActions));
                    break;
                
                //Play when resuming game
                case GameState.Play:
                    UpdateInputActions(true);
                    break;
                
                //can't register input actions when paused
                case GameState.Pause:
                    UpdateInputActions(false);
                    break;
            }
        }
        
        /// <summary>
        /// disable all input actions except Ui or
        /// enable all disabled actions
        /// this excludes all Ui actions
        /// </summary>
        /// <param name="enable"></param>
        private void UpdateInputActions(bool enable)
        {
            //disable all input actions except Ui on pause
            //enable back all disabled actions on resume/unpause
            foreach (InputAction inputAction in InputActions)
            {
                //skip ui actions
                if (inputAction.actionMap.name == nameof(InputActions.UI)) continue;
                    
                //disable on pause
                if (enable) inputAction.Enable();
                //enable on resume
                else inputAction.Disable();
            }
        }
    }
}
