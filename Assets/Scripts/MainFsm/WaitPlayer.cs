using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace  CliffLeeCL
{
    public class WaitPlayer : State<GameCore>
    {
        private bool isFinished;

        public override void OnStateEnter()
        {
            // WaitUntilAllPlayerJoins();
            WaitUntilStart();
        }



        public override void UpdateState()
        {
            if (isFinished)
            {
                stateContext.SwitchState("PreparePlayer");
            }
        }

        private async UniTask WaitUntilStart() 
        {
            var mainMenuUI = GameObject.Instantiate(GameConfig.Instance.mainMenuConfig.MainMenuUI);
            bool exitMenu = false;
            mainMenuUI.OnStart += () => exitMenu = true;
            await UniTask.WaitUntil(() => exitMenu);
            GameObject.Destroy(mainMenuUI.gameObject);
            isFinished = true;
        }

        private async UniTask WaitUntilAllPlayerJoins()
        {
            await stateContext.NetworkManager.WaitForAllPlayers();

            isFinished = true;
        }


        public override void OnStateExit()
        {
        }
    }
}
