using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace CliffLeeCL
{
    public class Entry : State<GameCore>
    {
        private AsyncOperation op;
        private bool isInitialized;

        public override void OnStateEnter()
        {
            Init().Forget();
        }

        private async UniTask Init()
        { 
            if (stateContext.IsUseNetwork)
            {
                await stateContext.NetworkManager.ConnectToServer();
            }
            isInitialized = true;
        }

        public override void UpdateState()
        {
            if (isInitialized)
            {   
                if (stateContext.IsUseNetwork)
                {
                    stateContext.SwitchState("WaitPlayer");
                }
                else
                {
                    stateContext.SwitchState("MainMenu");
                }
            }
        }

        public override void OnStateExit()
        {
        }
    }
}
