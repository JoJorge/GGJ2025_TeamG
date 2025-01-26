using UnityEngine;
using UnityEngine.SceneManagement;

namespace CliffLeeCL
{
    public class MatchProcess : State<GameCore>
    {
        InputSystem_Actions inputActions;

        public override void UpdateState()
        {
            inputActions.Enable();
            if (inputActions.Player.Exit.triggered)
            {
                Debug.LogError("Exit");
                stateContext.matchRoundTimer.StopCountDownTimer();
                EventManager.Instance.OnMatchOver();
                SceneManager.UnloadScene(SceneManager.GetSceneByName("Game"));
                stateContext.SwitchState("MainMenu");

            }
            if (inputActions.Player.Attack.triggered)
            {
                Debug.LogError("AUU");
            }
        }

        public override void OnStateEnter()
        {
            inputActions = new InputSystem_Actions();
            inputActions.Enable();

            stateContext.matchStartTimer.StartCountDownTimer(stateContext.matchStartTime, false, OnMatchStart);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            FieldManager.Instance.StartSpawnItem();
        }

        public override void OnStateExit()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        void OnMatchStart()
        {
            EventManager.Instance.OnMatchStart();
            stateContext.matchRoundTimer.StartCountDownTimer(stateContext.matchRoundTime, false, () =>
            {
                EventManager.Instance.OnMatchOver();
                stateContext.SwitchState("MatchOver");
            });
        }
    }
}
