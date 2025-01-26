using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace  CliffLeeCL
{
    public class MainMenu : State<GameCore>
    {
        private MainMenuUI mainMenuUI;
        private bool isFinished;

        public override void OnStateEnter()
        {
            mainMenuUI = GameObject.Instantiate(GameConfig.Instance.mainMenuConfig.MainMenuUI);
            mainMenuUI.OnStart += () => isFinished = true;
        }

        public override void UpdateState()
        {
            if (isFinished)
            {
                stateContext.SwitchState("PreparePlayer");
            }
        }

        public override void OnStateExit()
        {
            GameObject.Destroy(mainMenuUI.gameObject);
        }
    }
}
