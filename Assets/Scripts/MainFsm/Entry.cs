using UnityEngine;
using UnityEngine.SceneManagement;

namespace CliffLeeCL
{
    public class Entry : State<GameCore>
    {
        private AsyncOperation op;

        public override void OnStateEnter()
        {
            op = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
        }

        public override void UpdateState()
        {
            if (op.isDone)
            {
                stateContext.SwitchState("PreparePlayer");
            }
        }

        public override void OnStateExit()
        {
        }
    }
}
